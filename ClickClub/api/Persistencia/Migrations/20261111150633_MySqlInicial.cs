using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClickClub.Api.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class MySqlInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "atividades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nome = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descricao = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tipo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_atividades", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "categorias",
                columns: table => new
                {
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ativa = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ValorMensal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    LimiteDependentes = table.Column<int>(type: "int", nullable: true),
                    Beneficios = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias", x => x.name);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "clubes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nome = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Sigla = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Endereco = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Bairro = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Municipio = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Uf = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefone = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FusoHorario = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clubes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nome = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Login = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SenhaHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Papeis = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Clubes = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Atribuicoes = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cpf = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Dados_DataNascimento = table.Column<DateOnly>(type: "date", nullable: true),
                    Dados_Endereco = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Dados_Bairro = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Dados_Cidade = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Dados_Uf = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Dados_Sexo = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Dados_EstadoCivil = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Dados_Telefone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Dados_Email = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Dados_Interesses = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "avisos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClubeId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Titulo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Corpo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_avisos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_avisos_clubes_ClubeId",
                        column: x => x.ClubeId,
                        principalTable: "clubes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "espacos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClubeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nome = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tipo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Capacidade = table.Column<int>(type: "int", nullable: false),
                    Taxa = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_espacos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_espacos_clubes_ClubeId",
                        column: x => x.ClubeId,
                        principalTable: "clubes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "eventos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClubeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nome = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descricao = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Inicio = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Fim = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PublicoAlvo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Capacidade = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eventos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_eventos_clubes_ClubeId",
                        column: x => x.ClubeId,
                        principalTable: "clubes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "turmas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClubeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nome = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Inicio = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Fim = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Capacidade = table.Column<int>(type: "int", nullable: false),
                    Ativa = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_turmas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_turmas_clubes_ClubeId",
                        column: x => x.ClubeId,
                        principalTable: "clubes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "acessos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClubeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UsuarioId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    PorteiroId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DataHora = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Permitido = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Motivo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acessos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_acessos_clubes_ClubeId",
                        column: x => x.ClubeId,
                        principalTable: "clubes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_acessos_usuarios_PorteiroId",
                        column: x => x.PorteiroId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_acessos_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "associacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UsuarioTitularId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClubeOrigemId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Matricula = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Categoria = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TipoVinculo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ativa = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Dependentes = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_associacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_associacoes_categorias_Categoria",
                        column: x => x.Categoria,
                        principalTable: "categorias",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_associacoes_clubes_ClubeOrigemId",
                        column: x => x.ClubeOrigemId,
                        principalTable: "clubes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_associacoes_usuarios_UsuarioTitularId",
                        column: x => x.UsuarioTitularId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "auditoria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UsuarioId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Acao = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Entidade = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EntidadeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClubeId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    DataHora = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auditoria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_auditoria_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "chamados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UsuarioId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClubeId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Tipo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Assunto = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descricao = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Respostas = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chamados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_chamados_clubes_ClubeId",
                        column: x => x.ClubeId,
                        principalTable: "clubes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_chamados_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sessoes",
                columns: table => new
                {
                    Token = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false, collation: "ascii_bin"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Expires = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessoes", x => x.Token);
                    table.ForeignKey(
                        name: "FK_sessoes_usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "horarios_espacos",
                columns: table => new
                {
                    EspacoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    Inicio = table.Column<TimeOnly>(type: "time(6)", nullable: false),
                    Fim = table.Column<TimeOnly>(type: "time(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_horarios_espacos", x => new { x.EspacoId, x.DiaSemana, x.Inicio });
                    table.ForeignKey(
                        name: "FK_horarios_espacos_espacos_EspacoId",
                        column: x => x.EspacoId,
                        principalTable: "espacos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "interdicoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EspacoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Inicio = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Fim = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Motivo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_interdicoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_interdicoes_espacos_EspacoId",
                        column: x => x.EspacoId,
                        principalTable: "espacos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "inscricoes_eventos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EventoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UsuarioId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inscricoes_eventos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inscricoes_eventos_eventos_EventoId",
                        column: x => x.EventoId,
                        principalTable: "eventos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inscricoes_eventos_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "visitantes_eventos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EventoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nome = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Identificacao = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visitantes_eventos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_visitantes_eventos_eventos_EventoId",
                        column: x => x.EventoId,
                        principalTable: "eventos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "aulas",
                columns: table => new
                {
                    TurmaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Inicio = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Fim = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aulas", x => new { x.TurmaId, x.Inicio, x.Fim });
                    table.ForeignKey(
                        name: "FK_aulas_turmas_TurmaId",
                        column: x => x.TurmaId,
                        principalTable: "turmas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "inscricoes_turmas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TurmaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UsuarioId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InscritaEm = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PromovidoPor = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inscricoes_turmas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inscricoes_turmas_turmas_TurmaId",
                        column: x => x.TurmaId,
                        principalTable: "turmas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inscricoes_turmas_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "adesoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClubeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nome = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TipoVinculo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SolicitadaEm = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DecididaEm = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DecididaPor = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Motivo = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AssociacaoId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    UsuarioId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_adesoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_adesoes_associacoes_AssociacaoId",
                        column: x => x.AssociacaoId,
                        principalTable: "associacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_adesoes_clubes_ClubeId",
                        column: x => x.ClubeId,
                        principalTable: "clubes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_adesoes_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "autorizacoes_dependentes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AssociacaoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DependenteId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EspacoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Usada = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_autorizacoes_dependentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_autorizacoes_dependentes_associacoes_AssociacaoId",
                        column: x => x.AssociacaoId,
                        principalTable: "associacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_autorizacoes_dependentes_espacos_EspacoId",
                        column: x => x.EspacoId,
                        principalTable: "espacos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_autorizacoes_dependentes_usuarios_DependenteId",
                        column: x => x.DependenteId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "credenciais",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UsuarioId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AssociacaoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmitidaEm = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ValidaAte = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Revogada = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_credenciais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_credenciais_associacoes_AssociacaoId",
                        column: x => x.AssociacaoId,
                        principalTable: "associacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_credenciais_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "faturas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AssociacaoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Competencia = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Vencimento = table.Column<DateOnly>(type: "date", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_faturas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_faturas_associacoes_AssociacaoId",
                        column: x => x.AssociacaoId,
                        principalTable: "associacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "reservas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EspacoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AssociacaoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Inicio = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Fim = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Finalidade = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MesLocal = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StatusTaxa = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResponsavelId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reservas_associacoes_AssociacaoId",
                        column: x => x.AssociacaoId,
                        principalTable: "associacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reservas_espacos_EspacoId",
                        column: x => x.EspacoId,
                        principalTable: "espacos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reservas_usuarios_ResponsavelId",
                        column: x => x.ResponsavelId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "convidados_reservas",
                columns: table => new
                {
                    ReservaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Convidados = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_convidados_reservas", x => x.ReservaId);
                    table.ForeignKey(
                        name: "FK_convidados_reservas_reservas_ReservaId",
                        column: x => x.ReservaId,
                        principalTable: "reservas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "creditos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AssociacaoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ReservaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Descricao = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_creditos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_creditos_associacoes_AssociacaoId",
                        column: x => x.AssociacaoId,
                        principalTable: "associacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_creditos_reservas_ReservaId",
                        column: x => x.ReservaId,
                        principalTable: "reservas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "itens_faturas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FaturaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ReservaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Descricao = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_itens_faturas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_itens_faturas_faturas_FaturaId",
                        column: x => x.FaturaId,
                        principalTable: "faturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_itens_faturas_reservas_ReservaId",
                        column: x => x.ReservaId,
                        principalTable: "reservas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "creditos_aplicados",
                columns: table => new
                {
                    CreditoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_creditos_aplicados", x => x.CreditoId);
                    table.ForeignKey(
                        name: "FK_creditos_aplicados_creditos_CreditoId",
                        column: x => x.CreditoId,
                        principalTable: "creditos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_acessos_ClubeId",
                table: "acessos",
                column: "ClubeId");

            migrationBuilder.CreateIndex(
                name: "IX_acessos_PorteiroId",
                table: "acessos",
                column: "PorteiroId");

            migrationBuilder.CreateIndex(
                name: "IX_acessos_UsuarioId",
                table: "acessos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_adesoes_AssociacaoId",
                table: "adesoes",
                column: "AssociacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_adesoes_ClubeId",
                table: "adesoes",
                column: "ClubeId");

            migrationBuilder.CreateIndex(
                name: "IX_adesoes_UsuarioId",
                table: "adesoes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_associacoes_Categoria",
                table: "associacoes",
                column: "Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_associacoes_ClubeOrigemId",
                table: "associacoes",
                column: "ClubeOrigemId");

            migrationBuilder.CreateIndex(
                name: "IX_associacoes_Matricula",
                table: "associacoes",
                column: "Matricula",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_associacoes_UsuarioTitularId",
                table: "associacoes",
                column: "UsuarioTitularId");

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_UsuarioId",
                table: "auditoria",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_autorizacoes_dependentes_AssociacaoId",
                table: "autorizacoes_dependentes",
                column: "AssociacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_autorizacoes_dependentes_DependenteId",
                table: "autorizacoes_dependentes",
                column: "DependenteId");

            migrationBuilder.CreateIndex(
                name: "IX_autorizacoes_dependentes_EspacoId",
                table: "autorizacoes_dependentes",
                column: "EspacoId");

            migrationBuilder.CreateIndex(
                name: "IX_avisos_ClubeId",
                table: "avisos",
                column: "ClubeId");

            migrationBuilder.CreateIndex(
                name: "IX_chamados_ClubeId",
                table: "chamados",
                column: "ClubeId");

            migrationBuilder.CreateIndex(
                name: "IX_chamados_UsuarioId",
                table: "chamados",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_clubes_Sigla",
                table: "clubes",
                column: "Sigla",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_credenciais_AssociacaoId",
                table: "credenciais",
                column: "AssociacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_credenciais_UsuarioId",
                table: "credenciais",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_creditos_AssociacaoId",
                table: "creditos",
                column: "AssociacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_creditos_ReservaId",
                table: "creditos",
                column: "ReservaId");

            migrationBuilder.CreateIndex(
                name: "IX_espacos_ClubeId",
                table: "espacos",
                column: "ClubeId");

            migrationBuilder.CreateIndex(
                name: "IX_eventos_ClubeId",
                table: "eventos",
                column: "ClubeId");

            migrationBuilder.CreateIndex(
                name: "IX_faturas_AssociacaoId_Competencia",
                table: "faturas",
                columns: new[] { "AssociacaoId", "Competencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inscricoes_eventos_EventoId",
                table: "inscricoes_eventos",
                column: "EventoId");

            migrationBuilder.CreateIndex(
                name: "IX_inscricoes_eventos_UsuarioId",
                table: "inscricoes_eventos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_inscricoes_turmas_TurmaId",
                table: "inscricoes_turmas",
                column: "TurmaId");

            migrationBuilder.CreateIndex(
                name: "IX_inscricoes_turmas_UsuarioId",
                table: "inscricoes_turmas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_interdicoes_EspacoId",
                table: "interdicoes",
                column: "EspacoId");

            migrationBuilder.CreateIndex(
                name: "IX_itens_faturas_FaturaId",
                table: "itens_faturas",
                column: "FaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_itens_faturas_ReservaId",
                table: "itens_faturas",
                column: "ReservaId");

            migrationBuilder.CreateIndex(
                name: "IX_reservas_AssociacaoId",
                table: "reservas",
                column: "AssociacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_reservas_EspacoId_Inicio_Fim",
                table: "reservas",
                columns: new[] { "EspacoId", "Inicio", "Fim" });

            migrationBuilder.CreateIndex(
                name: "IX_reservas_ResponsavelId",
                table: "reservas",
                column: "ResponsavelId");

            migrationBuilder.CreateIndex(
                name: "IX_sessoes_UserId",
                table: "sessoes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_turmas_ClubeId",
                table: "turmas",
                column: "ClubeId");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_cpf",
                table: "usuarios",
                column: "cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Login",
                table: "usuarios",
                column: "Login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_visitantes_eventos_EventoId",
                table: "visitantes_eventos",
                column: "EventoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "acessos");

            migrationBuilder.DropTable(
                name: "adesoes");

            migrationBuilder.DropTable(
                name: "atividades");

            migrationBuilder.DropTable(
                name: "auditoria");

            migrationBuilder.DropTable(
                name: "aulas");

            migrationBuilder.DropTable(
                name: "autorizacoes_dependentes");

            migrationBuilder.DropTable(
                name: "avisos");

            migrationBuilder.DropTable(
                name: "chamados");

            migrationBuilder.DropTable(
                name: "convidados_reservas");

            migrationBuilder.DropTable(
                name: "credenciais");

            migrationBuilder.DropTable(
                name: "creditos_aplicados");

            migrationBuilder.DropTable(
                name: "horarios_espacos");

            migrationBuilder.DropTable(
                name: "inscricoes_eventos");

            migrationBuilder.DropTable(
                name: "inscricoes_turmas");

            migrationBuilder.DropTable(
                name: "interdicoes");

            migrationBuilder.DropTable(
                name: "itens_faturas");

            migrationBuilder.DropTable(
                name: "sessoes");

            migrationBuilder.DropTable(
                name: "visitantes_eventos");

            migrationBuilder.DropTable(
                name: "creditos");

            migrationBuilder.DropTable(
                name: "turmas");

            migrationBuilder.DropTable(
                name: "faturas");

            migrationBuilder.DropTable(
                name: "eventos");

            migrationBuilder.DropTable(
                name: "reservas");

            migrationBuilder.DropTable(
                name: "associacoes");

            migrationBuilder.DropTable(
                name: "espacos");

            migrationBuilder.DropTable(
                name: "categorias");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "clubes");
        }
    }
}
