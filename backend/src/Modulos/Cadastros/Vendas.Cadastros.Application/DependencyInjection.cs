using Microsoft.Extensions.DependencyInjection;
using Vendas.Cadastros.Application.Commands;
using Vendas.Cadastros.Application.Queries;

namespace Vendas.Cadastros.Application;

/// <summary>Registro dos handlers do módulo. Cada módulo expõe o seu — o Host só chama.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AdicionarCadastrosApplication(this IServiceCollection servicos)
    {
        servicos.AddScoped<CriarProdutoCommandHandler>();
        servicos.AddScoped<AtualizarProdutoCommandHandler>();
        servicos.AddScoped<ExcluirProdutoCommandHandler>();
        servicos.AddScoped<ListarProdutosQueryHandler>();
        servicos.AddScoped<ObterProdutoQueryHandler>();

        servicos.AddScoped<CriarClienteCommandHandler>();
        servicos.AddScoped<AtualizarClienteCommandHandler>();
        servicos.AddScoped<ExcluirClienteCommandHandler>();
        servicos.AddScoped<ListarClientesQueryHandler>();
        servicos.AddScoped<ObterClienteQueryHandler>();

        return servicos;
    }
}
