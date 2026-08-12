using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Vendas.Movimentos.Domain.Interfaces;
using Vendas.Movimentos.Infrastructure.Persistencia;
using Vendas.Movimentos.Infrastructure.Persistencia.Repositorios;
using Vendas.Movimentos.Infrastructure.Servicos;
using Vendas.Shared.Contratos;

namespace Vendas.Movimentos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AdicionarMovimentosInfrastructure(
        this IServiceCollection servicos, string stringDeConexao)
    {
        servicos.AddDbContext<MovimentosDbContext>(opcoes =>
            opcoes.UseSqlServer(stringDeConexao, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory_Movimentos")));

        servicos.AddScoped<ICompraRepositorio, CompraRepositorio>();
        servicos.AddScoped<IVendaRepositorio, VendaRepositorio>();
        servicos.AddScoped<IConsultaMovimentos, ConsultaMovimentos>();

        return servicos;
    }
}
