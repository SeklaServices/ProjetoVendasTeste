using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vendas.Cadastros.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class Cadastros_Clientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "SeqCliente");

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR SeqCliente"),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Documento = table.Column<string>(type: "nvarchar(18)", maxLength: 18, nullable: true),
                    Telefone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Codigo",
                table: "Clientes",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Documento",
                table: "Clientes",
                column: "Documento",
                unique: true,
                filter: "[Documento] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Nome",
                table: "Clientes",
                column: "Nome");

            // Cliente "não identificado": recebe as vendas que existiam quando o cliente era apenas
            // um texto digitado (D-009, pergunta B). O Id é fixo e conhecido pelos dois módulos via
            // Vendas.Shared.ClientesConhecidos — é assim que a migration de Movimentos consegue
            // apontar para um registro criado aqui sem consultar a tabela de outro DbContext.
            //
            // O IF NOT EXISTS deixa a migration idempotente: rodar duas vezes não duplica.
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Id = '9E1D0000-0000-4000-8000-000000000001')
                BEGIN
                    INSERT INTO Clientes (Id, Codigo, Nome, Documento, Telefone, Email, Ativo, DataCadastro)
                    VALUES (
                        '9E1D0000-0000-4000-8000-000000000001',
                        NEXT VALUE FOR SeqCliente,
                        'Cliente não identificado',
                        NULL, NULL, NULL,
                        1,
                        SYSUTCDATETIME());
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropSequence(
                name: "SeqCliente");
        }
    }
}
