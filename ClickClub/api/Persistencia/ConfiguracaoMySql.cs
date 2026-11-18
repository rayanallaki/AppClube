using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

static class ConfiguracaoMySql
{
    public static IServiceCollection AddClickClubMySql(this IServiceCollection services, IConfiguration config)
    {
        var connection = config.GetConnectionString("ClickClub");
        if (string.IsNullOrWhiteSpace(connection))
            throw new InvalidOperationException("Configure ConnectionStrings:ClickClub com dotnet user-secrets. Consulte INTEGRACAO_MYSQL.md.");
        byte[] key;
        try { key = Convert.FromBase64String(config["Credenciais:Chave"] ?? ""); }
        catch (FormatException) { throw new InvalidOperationException("Credenciais:Chave deve ser Base64 válido de 32 bytes."); }
        if (key.Length != 32)
            throw new InvalidOperationException("Configure Credenciais:Chave com uma chave de 32 bytes em Base64. Consulte INTEGRACAO_MYSQL.md.");
        Extras.ConfigurarChave(key);
        services.AddDbContext<ClickClubDbContext>(o => o.UseMySql(connection, new MySqlServerVersion(new Version(8, 0, 0))));
        services.AddScoped<Store>();
        return services;
    }

    public static async Task PrepararBancoAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClickClubDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        if (config.GetValue<bool>("Banco:AplicarMigracoes")) await db.Database.MigrateAsync();
        // Um banco não configurado/migrado falha no arranque; nunca cai em memória.
        await db.Set<Clube>().AnyAsync();
        if (config.GetValue<bool>("Banco:PopularDemonstracao"))
        {
            // A mesma trava das requisições impede duas instâncias de popular ao mesmo tempo.
            await db.Database.OpenConnectionAsync();
            await using var gate = await BancoLock.AcquireAsync(db, CancellationToken.None);
            await using var transaction = await db.Database.BeginTransactionAsync();
            if (!await db.Set<Usuario>().AnyAsync() && !await db.Set<Clube>().AnyAsync())
            {
                var store = scope.ServiceProvider.GetRequiredService<Store>();
                store.SeedDemo();
                await store.SaveAsync();
            }
            await transaction.CommitAsync();
        }
    }
}

// Permite gerar migrações e scripts sem conectar ao banco e sem exigir segredos.
class ClickClubDbContextFactory : IDesignTimeDbContextFactory<ClickClubDbContext>
{
    public ClickClubDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ClickClubDbContext>()
            .UseMySql("Server=localhost;Database=clickclub_dev;User=design_time;", new MySqlServerVersion(new Version(8, 0, 0)))
            .Options;
        return new ClickClubDbContext(options);
    }
}
