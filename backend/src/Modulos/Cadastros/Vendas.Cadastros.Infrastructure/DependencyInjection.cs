using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Vendas.Cadastros.Domain.Interfaces;
using Vendas.Cadastros.Infrastructure.Persistencia;
using Vendas.Cadastros.Infrastructure.Persistencia.Repositorios;
using Vendas.Cadastros.Infrastructure.Servicos;
using Vendas.Shared.Contratos;

namespace Vendas.Cadastros.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AdicionarCadastrosInfrastructure(
        this IServiceCollection servicos, string stringDeConexao)
    {
        servicos.AddDbContext<CadastrosDbContext>(opcoes =>
            opcoes.UseSqlServer(stringDeConexao, sql =>
                // Tabela de histórico própria por módulo: os dois DbContext compartilham o banco,
                // e sem isso um sobrescreveria o registro de migrations do outro.
                sql.MigrationsHistoryTable("__EFMigrationsHistory_Cadastros")));

        servicos.AddScoped<IProdutoRepositorio, ProdutoRepositorio>();
        servicos.AddScoped<IConsultaProdutos, ConsultaProdutos>();

        servicos.AddScoped<IClienteRepositorio, ClienteRepositorio>();
        servicos.AddScoped<IConsultaClientes, ConsultaClientes>();

        return servicos;
    }
}
