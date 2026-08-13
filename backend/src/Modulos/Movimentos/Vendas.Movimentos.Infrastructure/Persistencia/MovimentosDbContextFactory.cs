using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Vendas.Movimentos.Infrastructure.Persistencia;

/// <summary>Ver <c>CadastrosDbContextFactory</c> — mesma finalidade, para o outro módulo.</summary>
public sealed class MovimentosDbContextFactory : IDesignTimeDbContextFactory<MovimentosDbContext>
{
    public MovimentosDbContext CreateDbContext(string[] args)
    {
        var conexao = Environment.GetEnvironmentVariable("VENDAS_CONEXAO")
            ?? "Server=localhost;Database=ProjetoVendasTeste;Trusted_Connection=True;TrustServerCertificate=True";

        var opcoes = new DbContextOptionsBuilder<MovimentosDbContext>()
            .UseSqlServer(conexao, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_Movimentos"))
            .Options;

        return new MovimentosDbContext(opcoes);
    }
}
