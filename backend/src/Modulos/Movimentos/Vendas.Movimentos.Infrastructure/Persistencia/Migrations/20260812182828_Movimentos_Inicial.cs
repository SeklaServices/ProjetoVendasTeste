using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vendas.Movimentos.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class Movimentos_Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "SeqCompra");

            migrationBuilder.CreateSequence<int>(
                name: "SeqVenda");

            migrationBuilder.CreateTable(
                name: "Compras",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR SeqCompra"),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    Fornecedor = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Observacao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ValorTotal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Compras", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vendas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR SeqVenda"),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    Cliente = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Observacao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ValorTotal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComprasItens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProdutoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantidade = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComprasItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComprasItens_Compras_CompraId",
                        column: x => x.CompraId,
                        principalTable: "Compras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VendasItens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VendaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProdutoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantidade = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendasItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendasItens_Vendas_VendaId",
                        column: x => x.VendaId,
                        principalTable: "Vendas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Compras_Numero",
                table: "Compras",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComprasItens_CompraId",
                table: "ComprasItens",
                column: "CompraId");

            migrationBuilder.CreateIndex(
                name: "IX_ComprasItens_ProdutoId",
                table: "ComprasItens",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_Numero",
                table: "Vendas",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendasItens_ProdutoId",
                table: "VendasItens",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_VendasItens_VendaId",
                table: "VendasItens",
                column: "VendaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComprasItens");

            migrationBuilder.DropTable(
                name: "VendasItens");

            migrationBuilder.DropTable(
                name: "Compras");

            migrationBuilder.DropTable(
                name: "Vendas");

            migrationBuilder.DropSequence(
                name: "SeqCompra");

            migrationBuilder.DropSequence(
                name: "SeqVenda");
        }
    }
}
