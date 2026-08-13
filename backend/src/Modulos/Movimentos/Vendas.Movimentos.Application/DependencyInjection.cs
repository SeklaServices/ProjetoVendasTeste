using Microsoft.Extensions.DependencyInjection;
using Vendas.Movimentos.Application.Commands;
using Vendas.Movimentos.Application.Queries;

namespace Vendas.Movimentos.Application;

public static class DependencyInjection
{
    public static IServiceCollection AdicionarMovimentosApplication(this IServiceCollection servicos)
    {
        servicos.AddScoped<CriarCompraCommandHandler>();
        servicos.AddScoped<ExcluirCompraCommandHandler>();
        servicos.AddScoped<ListarComprasQueryHandler>();
        servicos.AddScoped<ObterCompraQueryHandler>();

        servicos.AddScoped<CriarVendaCommandHandler>();
        servicos.AddScoped<ExcluirVendaCommandHandler>();
        servicos.AddScoped<ListarVendasQueryHandler>();
        servicos.AddScoped<ObterVendaQueryHandler>();

        servicos.AddScoped<ResumoQueryHandler>();
        return servicos;
    }
}
