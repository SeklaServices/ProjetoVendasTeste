using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vendas.Movimentos.Infrastructure.Persistencia.Migrations
{
    /// <summary>
    /// Troca o texto livre `Vendas.Cliente` pelo `ClienteId`, apontando para o cadastro.
    ///
    /// A versão gerada automaticamente pelo `dotnet ef` apagava a coluna ANTES de migrar os dados e
    /// preenchia o `ClienteId` com `Guid.Empty` — o que deixaria toda venda existente apontando para
    /// um cliente que não existe. Esta versão foi escrita à mão para preservar os registros:
    /// cria a coluna anulável, aponta tudo para o "Cliente não identificado" (criado pela migration
    /// `Cadastros_Clientes`), só então torna obrigatória e descarta o texto antigo.
    ///
    /// Ver docs/07-decisoes.md D-009, pergunta B.
    /// </summary>
    public partial class Movimentos_VendaClienteId : Migration
    {
        private const string ClienteNaoIdentificado = "9E1D0000-0000-4000-8000-000000000001";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Anulável primeiro: as vendas existentes ainda não têm para onde apontar.
            migrationBuilder.AddColumn<Guid>(
                name: "ClienteId",
                table: "Vendas",
                type: "uniqueidentifier",
                nullable: true);

            // 2. Todas as vendas antigas passam a ser do cliente genérico. O nome que estava
            //    digitado vai para a observação, para a informação não se perder.
            migrationBuilder.Sql($"""
                UPDATE Vendas
                SET Observacao = CASE
                        WHEN Observacao IS NULL OR LTRIM(RTRIM(Observacao)) = ''
                            THEN LEFT('Cliente original: ' + Cliente, 500)
                        ELSE LEFT(Observacao + ' | Cliente original: ' + Cliente, 500)
                    END,
                    ClienteId = '{ClienteNaoIdentificado}'
                WHERE ClienteId IS NULL;
                """);

            // 3. Agora que ninguém está nulo, a coluna vira obrigatória. Sem defaultValue: uma venda
            //    nova SEM cliente tem que falhar, não cair silenciosamente no genérico.
            migrationBuilder.AlterColumn<Guid>(
                name: "ClienteId",
                table: "Vendas",
                type: "uniqueidentifier",
                nullable: false);

            // 4. Só depois o texto antigo é descartado.
            migrationBuilder.DropColumn(
                name: "Cliente",
                table: "Vendas");

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_ClienteId",
                table: "Vendas",
                column: "ClienteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vendas_ClienteId",
                table: "Vendas");

            migrationBuilder.AddColumn<string>(
                name: "Cliente",
                table: "Vendas",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            // A volta é possível, mas NÃO é simétrica: o nome digitado originalmente já foi
            // descartado no Up. Toda venda volta com o texto genérico.
            migrationBuilder.Sql(
                "UPDATE Vendas SET Cliente = 'Cliente não identificado' WHERE Cliente = '';");

            migrationBuilder.DropColumn(
                name: "ClienteId",
                table: "Vendas");
        }
    }
}
