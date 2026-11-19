using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using ClickClub.Mobile;
using SQLite;

var directory=Path.Combine(args.Length > 0 ? Path.GetFullPath(args[0]) : Path.GetTempPath(),"clickclub-mobile-tests-"+Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(directory);
var database=Path.Combine(directory,"cache.db3");
var secret=new TestStorage();
var handler=new TestHandler();
using var client=new MobileService(new HttpClient(handler),secret,database);
await client.LoginAsync("titular","senha-ficticia");
Assert(client.UserName=="Associado Teste","login e perfil");
Assert(secret.Secrets.ContainsKey("clickclub.session"),"sessão no armazenamento seguro abstrato");
await client.GetAsync("/api/avisos");
await client.GetAsync("/api/usuarios/usuario-teste/dados");
var online=await client.GetCardAsync();
Assert(!online.Offline&&!online.Expired&&online.QrSvg.Contains("svg"),"QR real gerado, credencial válida online");
handler.Offline=true;
var cached=await client.GetAsync("/api/avisos");
Assert(client.LastReadOffline&&cached.Array().Count==1,"mural recuperado de SQLite sem rede");
var offline=await client.GetCardAsync();
Assert(offline.Offline&&!offline.Expired,"carteirinha SQLite disponível offline");
await Throws(()=>client.GetAsync("/api/usuarios/usuario-teste/dados"),0,"dados pessoais não armazenados no cache");
var conn=new SQLiteAsyncConnection(database);
var rows=await conn.Table<CacheEntry>().ToListAsync();
Assert(rows.All(x=>!x.Json.Contains("token-secreto")&&!x.Json.Contains("senha-ficticia")&&!x.Json.Contains("cpf-privado")),"SQLite sem senha, token de sessão ou dados de perfil");

// A sessão dura oito horas; a credencial dura 24. A expiração da sessão
// não pode apagar uma carteirinha ainda válida quando o aparelho está offline.
var session=JsonNode.Parse(secret.Secrets["clickclub.session"])!.AsObject();
session["expires"]=DateTimeOffset.UtcNow.AddMinutes(-1).ToString("O");
secret.Secrets["clickclub.session"]=session.ToJsonString();
using var resumed=new MobileService(new HttpClient(handler),secret,database);
Assert(await resumed.ResumeAsync()&&resumed.SessionExpired,"perfil local recuperado com sessão expirada");
Assert(!(await resumed.GetCardAsync()).Expired,"credencial válida preservada após expirar sessão de login");
await Throws(()=>resumed.PostAsync("/api/reservas"),401,"operações online impedidas na sessão expirada");

// Credencial vencida é identificada, mesmo sem internet.
var saved=rows.Single(x=>x.Key.EndsWith("|carteirinha"));
var expiredCard=JsonNode.Parse(saved.Json)!.AsObject();
expiredCard["validaAte"]=DateTimeOffset.UtcNow.AddSeconds(-1).ToString("O");
saved.Json=expiredCard.ToJsonString();await conn.InsertOrReplaceAsync(saved);
Assert((await resumed.GetCardAsync()).Expired,"carteirinha vencida marcada como expirada offline");
await resumed.ConfigureUrlAsync("http://outro-servidor:5080/");
Assert(!resumed.SignedIn&&secret.Secrets.Count==0&&(await conn.Table<CacheEntry>().CountAsync())==0,"troca de servidor apaga sessão e cache");
await Throws(()=>resumed.ConfigureUrlAsync("https://host.example/path"),0,"URL com caminho rejeitada");
await Throws(()=>client.GetAsync("//host-externo.example/api"),0,"não encaminha token para URL externa");
await conn.CloseAsync();
Console.WriteLine("14 cenários do cliente mobile concluídos.");

static void Assert(bool value,string name){if(!value)throw new Exception("FALHOU: "+name);Console.WriteLine("OK: "+name);}
static async Task Throws(Func<Task> action,int status,string name)
{try{await action();throw new Exception("FALHOU: "+name);}catch(ApiException e){Assert(e.Status==status,name);}}

sealed class TestStorage:IDeviceStorage
{
    public string ApiUrl{get;set;}="http://localhost:5080/";
    public Dictionary<string,string> Secrets{get;}=new();
    public Task<string?> GetSecretAsync(string key)=>Task.FromResult(Secrets.GetValueOrDefault(key));
    public Task SetSecretAsync(string key,string value){Secrets[key]=value;return Task.CompletedTask;}
    public void RemoveSecret(string key)=>Secrets.Remove(key);
}
sealed class TestHandler:HttpMessageHandler
{
    public bool Offline;
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)
    {
        if(Offline)throw new HttpRequestException("rede indisponível");
        var result=request.RequestUri!.AbsolutePath switch
        {
            "/api/auth/login"=>new JsonObject{["token"]="token-secreto",["expiraEm"]=DateTimeOffset.UtcNow.AddHours(8).ToString("O"),["usuario"]=new JsonObject{["id"]="usuario-teste",["nome"]="Associado Teste",["papeis"]=new JsonArray("Titular")}}.ToJsonString(),
            "/api/carteirinha"=>new JsonObject{["credencial"]="credencial-de-acesso-ficticia",["validaAte"]=DateTimeOffset.UtcNow.AddHours(24).ToString("O")}.ToJsonString(),
            "/api/avisos"=>"[{\"titulo\":\"Encontro no clube\"}]",
            "/api/usuarios/usuario-teste/dados"=>"{\"dados\":{\"cpf\":\"cpf-privado\"}}",
            _=>"{}"
        };
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(result,Encoding.UTF8,"application/json")});
    }
}
