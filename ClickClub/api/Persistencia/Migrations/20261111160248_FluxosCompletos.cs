using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClickClub.Api.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class FluxosCompletos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AtividadeId",
                table: "turmas",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "turmas",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Instrutor",
                table: "turmas",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "CanceladaEm",
                table: "reservas",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Multa",
                table: "reservas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Reembolso",
                table: "reservas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorBase",
                table: "faturas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            // Preserva as mensalidades do pacote MySQL anterior, cujo total já
            // podia conter locações, acréscimos ou créditos discriminados.
            migrationBuilder.Sql(@"
                UPDATE faturas f
                LEFT JOIN (SELECT FaturaId, SUM(Valor) valorItens FROM itens_faturas GROUP BY FaturaId) i
                  ON i.FaturaId = f.Id
                SET f.ValorBase = f.Valor - COALESCE(i.valorItens, 0);
            ");

            migrationBuilder.CreateIndex(
                name: "IX_turmas_AtividadeId",
                table: "turmas",
                column: "AtividadeId");

            migrationBuilder.AddForeignKey(
                name: "FK_turmas_atividades_AtividadeId",
                table: "turmas",
                column: "AtividadeId",
                principalTable: "atividades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_turmas_atividades_AtividadeId",
                table: "turmas");

            migrationBuilder.DropIndex(
                name: "IX_turmas_AtividadeId",
                table: "turmas");

            migrationBuilder.DropColumn(
                name: "AtividadeId",
                table: "turmas");

            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "turmas");

            migrationBuilder.DropColumn(
                name: "Instrutor",
                table: "turmas");

            migrationBuilder.DropColumn(
                name: "CanceladaEm",
                table: "reservas");

            migrationBuilder.DropColumn(
                name: "Multa",
                table: "reservas");

            migrationBuilder.DropColumn(
                name: "Reembolso",
                table: "reservas");

            migrationBuilder.DropColumn(
                name: "ValorBase",
                table: "faturas");
        }
    }
}
