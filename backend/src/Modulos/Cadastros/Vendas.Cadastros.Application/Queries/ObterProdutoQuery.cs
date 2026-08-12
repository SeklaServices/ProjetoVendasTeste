using Vendas.Cadastros.Application.Dtos;
using Vendas.Cadastros.Domain.Interfaces;

namespace Vendas.Cadastros.Application.Queries;

public sealed class ObterProdutoQueryHandler(IProdutoRepositorio repositorio)
{
    public async Task<ProdutoDto?> HandleAsync(Guid id, CancellationToken ct)
    {
        var produto = await repositorio.ObterPorIdAsync(id, ct);
        return produto is null ? null : ListarProdutosQueryHandler.Mapear(produto);
    }
}
