using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

// Cada entidade principal tem sua tabela. Coleções internas pequenas são JSON
// nesta primeira integração, para preservar o contrato dos endpoints existentes.
class ClickClubDbContext(DbContextOptions<ClickClubDbContext> options) : DbContext(options)
{
    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder.Properties<decimal>().HavePrecision(18, 2);
        builder.Properties<DateTimeOffset>().HaveConversion<InstanteUtcConverter>();
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasCharSet("utf8mb4");
        Table<Usuario>(b, "usuarios");
        var user = b.Entity<Usuario>();
        user.Property(x => x.Login).HasMaxLength(100);
        user.HasIndex(x => x.Login).IsUnique();
        Json(user.Property(x => x.Papeis));
        Json(user.Property(x => x.Clubes));
        Json(user.Property(x => x.Atribuicoes));
        user.OwnsOne(x => x.Dados, dados =>
        {
            dados.Property(x => x.Cpf).HasColumnName("cpf").HasMaxLength(11);
            dados.HasIndex(x => x.Cpf).IsUnique();
            Json(dados.Property(x => x.Interesses));
        });
        user.Navigation(x => x.Dados).IsRequired();

        Table<Clube>(b, "clubes");
        b.Entity<Clube>().Property(x => x.Sigla).HasMaxLength(100);
        b.Entity<Clube>().HasIndex(x => x.Sigla).IsUnique();

        b.Entity<Categoria>().ToTable("categorias").HasKey(x => x.name);
        b.Entity<Categoria>().Property(x => x.name).HasMaxLength(100);

        Table<Associacao>(b, "associacoes");
        b.Entity<Associacao>().Property(x => x.Matricula).HasMaxLength(100);
        b.Entity<Associacao>().HasIndex(x => x.Matricula).IsUnique();
        b.Entity<Associacao>().Property(x => x.Categoria).HasMaxLength(100);
        Json(b.Entity<Associacao>().Property(x => x.Dependentes));
        b.Entity<Associacao>().HasOne<Usuario>().WithMany().HasForeignKey(x => x.UsuarioTitularId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Associacao>().HasOne<Clube>().WithMany().HasForeignKey(x => x.ClubeOrigemId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Associacao>().HasOne<Categoria>().WithMany().HasForeignKey(x => x.Categoria).OnDelete(DeleteBehavior.Restrict);

        Table<Adesao>(b, "adesoes");
        b.Entity<Adesao>().HasOne<Clube>().WithMany().HasForeignKey(x => x.ClubeId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Adesao>().HasOne<Usuario>().WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Adesao>().HasOne<Associacao>().WithMany().HasForeignKey(x => x.AssociacaoId).OnDelete(DeleteBehavior.Restrict);

        Table<Espaco>(b, "espacos");
        b.Entity<Espaco>().HasOne<Clube>().WithMany().HasForeignKey(x => x.ClubeId).OnDelete(DeleteBehavior.Restrict);
        Table<Reserva>(b, "reservas");
        b.Entity<Reserva>().HasOne<Espaco>().WithMany().HasForeignKey(x => x.EspacoId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Reserva>().HasOne<Associacao>().WithMany().HasForeignKey(x => x.AssociacaoId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Reserva>().HasOne<Usuario>().WithMany().HasForeignKey(x => x.ResponsavelId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Reserva>().HasIndex(x => new { x.EspacoId, x.Inicio, x.Fim });
        Table<Fatura>(b, "faturas");
        b.Entity<Fatura>().Property(x => x.Competencia).HasMaxLength(7);
        b.Entity<Fatura>().HasIndex(x => new { x.AssociacaoId, x.Competencia }).IsUnique();
        b.Entity<Fatura>().HasOne<Associacao>().WithMany().HasForeignKey(x => x.AssociacaoId).OnDelete(DeleteBehavior.Restrict);
        Table<Aviso>(b, "avisos");
        b.Entity<Aviso>().HasOne<Clube>().WithMany().HasForeignKey(x => x.ClubeId).OnDelete(DeleteBehavior.Restrict);
        Table<Chamado>(b, "chamados");
        Json(b.Entity<Chamado>().Property(x => x.Respostas));
        b.Entity<Chamado>().HasOne<Usuario>().WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Chamado>().HasOne<Clube>().WithMany().HasForeignKey(x => x.ClubeId).OnDelete(DeleteBehavior.Restrict);
        Table<Evento>(b, "eventos");
        b.Entity<Evento>().HasOne<Clube>().WithMany().HasForeignKey(x => x.ClubeId).OnDelete(DeleteBehavior.Restrict);
        Table<Turma>(b, "turmas");
        b.Entity<Turma>().HasOne<Atividade>().WithMany().HasForeignKey(x=>x.AtividadeId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Turma>().HasOne<Clube>().WithMany().HasForeignKey(x => x.ClubeId).OnDelete(DeleteBehavior.Restrict);
        Table<Inscricao>(b, "inscricoes_turmas");
        b.Entity<Inscricao>().HasOne<Turma>().WithMany().HasForeignKey(x => x.TurmaId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Inscricao>().HasOne<Usuario>().WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        Table<Credencial>(b, "credenciais");
        b.Entity<Credencial>().HasOne<Usuario>().WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Credencial>().HasOne<Associacao>().WithMany().HasForeignKey(x => x.AssociacaoId).OnDelete(DeleteBehavior.Restrict);
        Table<RegistroPortaria>(b, "acessos");
        b.Entity<RegistroPortaria>().HasOne<Clube>().WithMany().HasForeignKey(x => x.ClubeId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<RegistroPortaria>().HasOne<Usuario>().WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<RegistroPortaria>().HasOne<Usuario>().WithMany().HasForeignKey(x => x.PorteiroId).OnDelete(DeleteBehavior.Restrict);
        Table<ItemTransferido>(b, "itens_faturas");
        b.Entity<ItemTransferido>().HasOne<Fatura>().WithMany().HasForeignKey(x => x.FaturaId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<ItemTransferido>().HasOne<Reserva>().WithMany().HasForeignKey(x => x.ReservaId).OnDelete(DeleteBehavior.Restrict);
        Table<Credito>(b, "creditos");
        b.Entity<Credito>().HasOne<Associacao>().WithMany().HasForeignKey(x => x.AssociacaoId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Credito>().HasOne<Reserva>().WithMany().HasForeignKey(x => x.ReservaId).OnDelete(DeleteBehavior.Restrict);
        Table<Interdicao>(b, "interdicoes");
        b.Entity<Interdicao>().HasOne<Espaco>().WithMany().HasForeignKey(x => x.EspacoId).OnDelete(DeleteBehavior.Restrict);
        Table<AutorizacaoDependente>(b, "autorizacoes_dependentes");
        b.Entity<AutorizacaoDependente>().HasOne<Associacao>().WithMany().HasForeignKey(x => x.AssociacaoId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<AutorizacaoDependente>().HasOne<Usuario>().WithMany().HasForeignKey(x => x.DependenteId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<AutorizacaoDependente>().HasOne<Espaco>().WithMany().HasForeignKey(x => x.EspacoId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<HorarioFuncionamento>().ToTable("horarios_espacos").HasKey(x => new { x.EspacoId, x.DiaSemana, x.Inicio });
        b.Entity<HorarioFuncionamento>().HasOne<Espaco>().WithMany().HasForeignKey(x => x.EspacoId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Aula>().ToTable("aulas").HasKey(x => new { x.TurmaId, x.Inicio, x.Fim });
        b.Entity<Aula>().HasOne<Turma>().WithMany().HasForeignKey(x => x.TurmaId).OnDelete(DeleteBehavior.Restrict);
        Table<Atividade>(b, "atividades");
        Table<EntradaVisitante>(b, "visitantes_eventos");
        b.Entity<EntradaVisitante>().HasOne<Evento>().WithMany().HasForeignKey(x => x.EventoId).OnDelete(DeleteBehavior.Restrict);
        Table<InscricaoEvento>(b, "inscricoes_eventos");
        b.Entity<InscricaoEvento>().HasOne<Evento>().WithMany().HasForeignKey(x => x.EventoId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<InscricaoEvento>().HasOne<Usuario>().WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<SessaoPersistida>().ToTable("sessoes").HasKey(x => x.Token);
        b.Entity<SessaoPersistida>().Property(x => x.Token).HasMaxLength(64).UseCollation("ascii_bin");
        b.Entity<SessaoPersistida>().HasOne<Usuario>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<ListaConvidados>().ToTable("convidados_reservas").HasKey(x => x.ReservaId);
        Json(b.Entity<ListaConvidados>().Property(x => x.Convidados));
        b.Entity<ListaConvidados>().HasOne<Reserva>().WithMany().HasForeignKey(x => x.ReservaId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<CreditoAplicado>().ToTable("creditos_aplicados").HasKey(x => x.CreditoId);
        b.Entity<CreditoAplicado>().HasOne<Credito>().WithMany().HasForeignKey(x => x.CreditoId).OnDelete(DeleteBehavior.Restrict);
        Table<RegistroAuditoria>(b, "auditoria");
        b.Entity<RegistroAuditoria>().HasOne<Usuario>().WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
    }

    static void Table<T>(ModelBuilder b, string name) where T : class => b.Entity<T>().ToTable(name).HasKey("Id");

    static void Json<T>(PropertyBuilder<T> property) where T : class
    {
        property.HasConversion(
            value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
            value => JsonSerializer.Deserialize<T>(value, (JsonSerializerOptions?)null)!);
        property.Metadata.SetValueComparer(new ValueComparer<T>(
            (a, c) => JsonSerializer.Serialize(a, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(c, (JsonSerializerOptions?)null),
            value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null).GetHashCode(),
            value => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, (JsonSerializerOptions?)null), (JsonSerializerOptions?)null)!));
        property.HasColumnType("json");
    }
}

class InstanteUtcConverter() : ValueConverter<DateTimeOffset, DateTime>(
    value => value.UtcDateTime,
    value => new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc)));

record SessaoPersistida(string Token, Guid UserId, DateTimeOffset Expires);
record ListaConvidados(Guid ReservaId, List<Convidado> Convidados);
record CreditoAplicado(Guid CreditoId);
record RegistroAuditoria(Guid Id, Guid UsuarioId, string Acao, string Entidade, Guid EntidadeId, Guid? ClubeId, DateTimeOffset DataHora);
