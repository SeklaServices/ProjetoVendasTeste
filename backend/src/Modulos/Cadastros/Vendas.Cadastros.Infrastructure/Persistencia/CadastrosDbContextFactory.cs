using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Vendas.Cadastros.Infrastructure.Persistencia;

/// <summary>
/// Usada só pelo <c>dotnet ef</c> ao gerar migrations. Sem ela, a ferramenta teria que subir o Host
/// inteiro — e o Host aplica migrations no boot, o que criaria uma dependência circular boba.
///
/// A conexão daqui não precisa apontar para um banco existente: gerar migration é offline. Para
/// <c>database update</c>, quem manda é a string do appsettings.Development.json de cada um.
/// </summary>
public sealed class CadastrosDbContextFactory : IDesignTimeDbContextFactory<CadastrosDbContext>
{
    public CadastrosDbContext CreateDbContext(string[] args)
    {
        var conexao = Environment.GetEnvironmentVariable("VENDAS_CONEXAO")
            ?? "Server=localhost;Database=ProjetoVendasTeste;Trusted_Connection=True;TrustServerCertificate=True";

        var opcoes = new DbContextOptionsBuilder<CadastrosDbContext>()
            .UseSqlServer(conexao, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_Cadastros"))
            .Options;

        return new CadastrosDbContext(opcoes);
    }
}
