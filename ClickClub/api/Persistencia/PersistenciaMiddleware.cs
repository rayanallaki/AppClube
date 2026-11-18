using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

// O Store original faz verificações e alterações em várias coleções. Nesta fase
// uma trava no MySQL serializa as operações inclusive entre processos da API.
// A resposta só é enviada depois do commit. Erros descartam o Store da requisição.
class PersistenciaMiddleware(RequestDelegate next, ILogger<PersistenciaMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext http, ClickClubDbContext db)
    {
        if (!http.Request.Path.StartsWithSegments("/api") || http.Request.Path == "/api/saude")
        {
            await next(http);
            return;
        }
        var original = http.Response.Body;
        await using var buffer = new MemoryStream();
        http.Response.Body = buffer;
        try
        {
            await db.Database.OpenConnectionAsync(http.RequestAborted);
            await using var gate = await BancoLock.AcquireAsync(db, http.RequestAborted);
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, http.RequestAborted);
            await next(http);
            if (http.Response.StatusCode is >= 200 and < 400)
            {
                if (!HttpMethods.IsGet(http.Request.Method) && !HttpMethods.IsHead(http.Request.Method) && !HttpMethods.IsOptions(http.Request.Method))
                    await http.RequestServices.GetRequiredService<Store>().SaveAsync(http.RequestAborted);
                await transaction.CommitAsync(http.RequestAborted);
            }
            else await transaction.RollbackAsync(http.RequestAborted);
        }
        catch (Exception e) when (e is MySqlException or DbUpdateException or TimeoutException)
        {
            logger.LogError("Operação MySQL não concluída. Tipo: {Tipo}", e.GetType().Name);
            buffer.SetLength(0);
            http.Response.Clear();
            var duplicate = e is DbUpdateException { InnerException: MySqlException { Number: 1062 } };
            http.Response.StatusCode = duplicate ? 409 : 503;
            await http.Response.WriteAsJsonAsync(new { erro = duplicate
                ? "Registro duplicado. Confira os dados e tente novamente."
                : "Não foi possível confirmar a operação no MySQL. Consulte os dados antes de repetir." }, http.RequestAborted);
        }
        finally { http.Response.Body = original; }
        buffer.Position = 0;
        await buffer.CopyToAsync(original, http.RequestAborted);
    }
}

sealed class BancoLock(ClickClubDbContext db, string name) : IAsyncDisposable
{
    public static async Task<BancoLock> AcquireAsync(ClickClubDbContext db, CancellationToken cancellationToken)
    {
        var connection = db.Database.GetDbConnection();
        var name = "clickclub:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(connection.Database)))[..32];
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT GET_LOCK(@name, 15)";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@name";
        parameter.Value = name;
        command.Parameters.Add(parameter);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        if (result is null || result is DBNull || Convert.ToInt32(result) != 1)
            throw new TimeoutException("O banco está ocupado. Tente novamente.");
        return new BancoLock(db, name);
    }

    public async ValueTask DisposeAsync()
    {
        // Se a conexão caiu, o próprio servidor já liberou a trava.
        var connection = db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open) return;
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT RELEASE_LOCK(@name)";
        command.CommandTimeout = 5;
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@name";
        parameter.Value = name;
        command.Parameters.Add(parameter);
        try { await command.ExecuteScalarAsync(CancellationToken.None); }
        catch (MySqlException) { MySqlConnection.ClearPool((MySqlConnection)connection); }
    }
}
