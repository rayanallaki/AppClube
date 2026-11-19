using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using QRCoder;
using SQLite;

namespace ClickClub.Mobile;

public interface IDeviceStorage
{
    string ApiUrl { get; set; }
    Task<string?> GetSecretAsync(string key);
    Task SetSecretAsync(string key, string value);
    void RemoveSecret(string key);
}

public sealed class ApiException(string message, int status = 0) : Exception(message)
{
    public int Status { get; } = status;
}

[Table("cache_local")]
public sealed class CacheEntry
{
    [PrimaryKey] public string Key { get; set; } = "";
    public string Account { get; set; } = "";
    public string Json { get; set; } = "";
    public DateTime UpdatedUtc { get; set; }
}

public sealed record CardView(JsonObject Data, bool Offline, bool Expired, string QrSvg);

/// <summary>Cliente da API: segredos no SO, cache SQLite isolado por conta e servidor.</summary>
public sealed class MobileService : IDisposable
{
    public const string DefaultApiUrl = "http://localhost:5080/";

    readonly HttpClient http;
    readonly IDeviceStorage storage;
    readonly SQLiteAsyncConnection cache;
    readonly SemaphoreSlim cacheInit = new(1, 1);
    bool initialized;
    string? token;
    DateTimeOffset expires;
    public JsonObject? User { get; private set; }
    public bool LastReadOffline { get; private set; }
    public string ApiUrl => storage.ApiUrl;
    public bool SignedIn => User is not null && token is not null;
    public bool SessionExpired => SignedIn && expires <= DateTimeOffset.UtcNow;
    public bool Candidate => HasRole("Candidato");
    public bool Dependent => HasRole("Dependente") && !HasRole("Titular");
    public string UserId => User.Text("id");
    public string UserName => User.Text("nome");
    public bool HasRole(string role) => User?["papeis"] is JsonArray roles && roles.Any(x => x?.GetValue<string>() == role);
    string Account => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(ApiUrl + "|" + UserId)));

    public MobileService(HttpClient http, IDeviceStorage storage, string databasePath)
    {
        this.http = http;
        this.storage = storage;
        this.http.Timeout = TimeSpan.FromSeconds(15);
        cache = new SQLiteAsyncConnection(databasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);
    }

    async Task InitAsync()
    {
        if (initialized) return;
        await cacheInit.WaitAsync();
        try
        {
            if (!initialized) { await cache.CreateTableAsync<CacheEntry>(); initialized = true; }
        }
        finally { cacheInit.Release(); }
    }

    public async Task ConfigureUrlAsync(string address)
    {
        if (!Uri.TryCreate(address.Trim(), UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https") || !string.IsNullOrEmpty(uri.UserInfo) || uri.AbsolutePath != "/" || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
            throw new ApiException($"Informe somente o endereço da API, como {DefaultApiUrl}");
        var normalized = uri.GetLeftPart(UriPartial.Authority) + "/";
        if (normalized != storage.ApiUrl) { await LogoutAsync(false); storage.ApiUrl = normalized; }
    }

    public async Task<bool> ResumeAsync()
    {
        try
        {
            var raw = await storage.GetSecretAsync("clickclub.session");
            if (string.IsNullOrWhiteSpace(raw)) return false;
            var session = JsonNode.Parse(raw)!.AsObject();
            if (session.Text("server") != ApiUrl) return false;
            expires = DateTimeOffset.Parse(session.Text("expires"));
            token = session.Text("token");
            User = session["user"]!.DeepClone().AsObject();
            if (SessionExpired) { LastReadOffline = true; return true; }
            try { User = (await GetAsync("/api/auth/eu", false)).AsObject(); }
            catch (ApiException e) when (e.Status == 0) { LastReadOffline = true; }
            return SignedIn;
        }
        catch { await LogoutAsync(false); return false; }
    }

    public async Task LoginAsync(string login, string password)
    {
        await LogoutAsync(false);
        var result = (await SendAsync(HttpMethod.Post, "/api/auth/login", new { login, senha = password }, false)).AsObject();
        token = result.Text("token");
        expires = DateTimeOffset.Parse(result.Text("expiraEm"));
        User = result["usuario"]!.DeepClone().AsObject();
        await storage.SetSecretAsync("clickclub.session", new JsonObject
        {
            ["server"] = ApiUrl, ["token"] = token, ["expires"] = expires.ToString("O"), ["user"] = User.DeepClone()
        }.ToJsonString());
    }

    public async Task LogoutAsync(bool revoke = true)
    {
        if (revoke && SignedIn)
        {
            try { await SendAsync(HttpMethod.Post, "/api/auth/sair", new { }); } catch { /* Limpeza local sempre ocorre. */ }
        }
        await InitAsync();
        await cache.DeleteAllAsync<CacheEntry>();
        token = null; User = null; expires = default;
        storage.RemoveSecret("clickclub.session");
    }

    public async Task<JsonNode> GetAsync(string path, bool allowCache = true, bool authenticated = true)
    {
        LastReadOffline = false;
        try
        {
            var result = await SendAsync(HttpMethod.Get, path, null, authenticated);
            if (allowCache && authenticated && SignedIn && CacheAllowed(path)) await SaveCacheAsync(path, result);
            return result;
        }
        catch (ApiException e) when (e.Status == 0 && allowCache && SignedIn && CacheAllowed(path))
        {
            var saved = await ReadCacheAsync(path);
            if (saved is null) throw;
            LastReadOffline = true;
            return saved;
        }
    }

    public Task<JsonNode> PostAsync(string path, object? data = null) => SendAsync(HttpMethod.Post, path, data ?? new { });
    public Task<JsonNode> PutAsync(string path, object data) => SendAsync(HttpMethod.Put, path, data);
    public Task<JsonNode> DeleteAsync(string path) => SendAsync(HttpMethod.Delete, path, null);

    public async Task<JsonNode> SendAsync(HttpMethod method, string path, object? data, bool authenticated = true)
    {
        // Caminhos absolutos externos são recusados para nunca encaminhar um token a outro host.
        if (!path.StartsWith('/') || path.StartsWith("//") || path.Contains('\\')) throw new ApiException("Caminho da operação inválido.");
        using var request = new HttpRequestMessage(method, new Uri(new Uri(ApiUrl), path));
        if (authenticated)
        {
            if (!SignedIn) throw new ApiException("Entre com sua conta para continuar.", 401);
            if (SessionExpired) throw new ApiException("Sua sessão expirou. Saia e entre novamente para operações online. A carteirinha armazenada continua disponível até sua própria validade.", 401);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        if (data is not null) request.Content = JsonContent.Create(data);
        try
        {
            using var response = await http.SendAsync(request);
            var text = await response.Content.ReadAsStringAsync();
            JsonNode? json = null;
            try { if (!string.IsNullOrWhiteSpace(text)) json = JsonNode.Parse(text); } catch (JsonException) { }
            if (!response.IsSuccessStatusCode)
            {
                var status = (int)response.StatusCode;
                if (status == 401 && authenticated) await LogoutAsync(false);
                var message = json?.Text("erro");
                if (string.IsNullOrWhiteSpace(message)) message = status switch
                {
                    401 => "Login ou senha inválidos, ou sessão expirada.",
                    403 => "Seu perfil não possui permissão para esta operação.",
                    404 => "O registro solicitado não foi encontrado.",
                    429 => "Muitas tentativas. Aguarde um momento e tente novamente.",
                    _ => "A API não conseguiu concluir a operação. Confira os dados e tente novamente."
                };
                throw new ApiException(message, status);
            }
            return json ?? new JsonObject();
        }
        catch (HttpRequestException) { throw new ApiException("Sem conexão com a API. Confira a rede e o endereço configurado."); }
        catch (TaskCanceledException) { throw new ApiException("A API demorou para responder. Tente novamente quando a conexão voltar."); }
    }

    static bool CacheAllowed(string path) => path is "/api/avisos" or "/api/clubes" or "/api/espacos" or "/api/turmas" or "/api/eventos" or "/api/reservas";

    async Task SaveCacheAsync(string path, JsonNode data)
    {
        await InitAsync();
        await cache.InsertOrReplaceAsync(new CacheEntry { Key = Account + "|" + path, Account = Account, Json = data.ToJsonString(), UpdatedUtc = DateTime.UtcNow });
    }

    async Task<JsonNode?> ReadCacheAsync(string path)
    {
        await InitAsync();
        var row = await cache.FindAsync<CacheEntry>(Account + "|" + path);
        return row is null ? null : JsonNode.Parse(row.Json);
    }

    public async Task<CardView> GetCardAsync()
    {
        JsonObject card; var offline = false;
        try
        {
            if (SessionExpired) throw new ApiException("Sessão expirada; exibindo a carteirinha local.");
            card = (await PostAsync("/api/carteirinha")).AsObject();
            await SaveCacheAsync("carteirinha", card);
        }
        catch (ApiException e) when (e.Status == 0)
        {
            var saved = await ReadCacheAsync("carteirinha");
            if (saved is null) throw new ApiException("Conecte-se uma vez para emitir sua carteirinha. Depois ela ficará disponível offline até expirar.");
            card = saved.AsObject(); offline = true;
        }
        var expired = !DateTimeOffset.TryParse(card.Text("validaAte"), out var until) || until <= DateTimeOffset.UtcNow;
        var svg = card.Text("qrSvg");
        if (string.IsNullOrWhiteSpace(svg))
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(card.Text("credencial"), QRCodeGenerator.ECCLevel.Q);
            using var qr = new SvgQRCode(data);
            svg = qr.GetGraphic(6);
        }
        return new CardView(card, offline, expired, svg);
    }

    public async Task ClearCacheAsync() { await InitAsync(); await cache.DeleteAllAsync<CacheEntry>(); }
    public void Dispose() { http.Dispose(); cacheInit.Dispose(); }
}

public static class JsonExtensions
{
    public static string Text(this JsonNode? node, string property, string fallback = "")
    {
        if (node is not JsonObject obj || !obj.TryGetPropertyValue(property, out var value) || value is null) return fallback;
        return value is JsonValue v && v.TryGetValue<string>(out var s) ? s : value.ToJsonString();
    }
    public static JsonArray Array(this JsonNode? node) => node as JsonArray ?? new JsonArray();
    public static bool Flag(this JsonNode? node, string property) => bool.TryParse(node.Text(property), out var b) && b;
}
