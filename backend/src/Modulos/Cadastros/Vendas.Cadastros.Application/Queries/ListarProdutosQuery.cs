using Vendas.Cadastros.Application.Dtos;
using Vendas.Cadastros.Domain.Entidades;
using Vendas.Cadastros.Domain.Interfaces;

namespace Vendas.Cadastros.Application.Queries;

public sealed record ListarProdutosQuery(string? Busca, bool ApenasAtivos);

public sealed class ListarProdutosQueryHandler(IProdutoRepositorio repositorio)
{
    public async Task<IReadOnlyList<ProdutoDto>> HandleAsync(ListarProdutosQuery consulta, CancellationToken ct)
    {
        var produtos = await repositorio.ListarAsync(consulta.Busca, consulta.ApenasAtivos, ct);
        return produtos.Select(Mapear).ToList();
    }

    internal static ProdutoDto Mapear(Produto p) => new(
        p.Id, p.Codigo, p.Nome, p.UnidadeMedida, p.PrecoCusto, p.PrecoVenda, p.Ativo, p.DataCadastro);
}
