using Vendas.Movimentos.Application.Dtos;
using Vendas.Movimentos.Domain.Entidades;
using Vendas.Movimentos.Domain.Interfaces;
using Vendas.Shared.Contratos;

namespace Vendas.Movimentos.Application.Queries;

public sealed record ListarComprasQuery(DateOnly? DataInicial, DateOnly? DataFinal);

public sealed class ListarComprasQueryHandler(ICompraRepositorio repositorio)
{
    public async Task<IReadOnlyList<CompraResumoDto>> HandleAsync(ListarComprasQuery consulta, CancellationToken ct)
    {
        var compras = await repositorio.ListarAsync(consulta.DataInicial, consulta.DataFinal, ct);
        return compras.Select(MapearResumo).ToList();
    }

    internal static CompraResumoDto MapearResumo(Compra c) =>
        new(c.Id, c.Numero, c.Data, c.Fornecedor, c.ValorTotal, c.Itens.Count);
}

public sealed class ObterCompraQueryHandler(
    ICompraRepositorio repositorio,
    IConsultaProdutos consultaProdutos)
{
    public async Task<CompraDetalheDto?> HandleAsync(Guid id, CancellationToken ct)
    {
        var compra = await repositorio.ObterPorIdAsync(id, ct);
        if (compra is null)
        {
            return null;
        }

        // Nome e código do produto vêm do módulo Cadastros pelo contrato — nunca por join entre
        // DbContexts diferentes.
        var produtos = await consultaProdutos.ObterPorIdsAsync(
            compra.Itens.Select(i => i.ProdutoId).Distinct().ToList(), ct);

        var itens = compra.Itens
            .Select(i => MapeadorDeItem.Mapear(
                i.Id, i.ProdutoId, i.Quantidade, i.PrecoUnitario, i.Subtotal, produtos))
            .ToList();

        return new CompraDetalheDto(
            compra.Id, compra.Numero, compra.Data, compra.Fornecedor, compra.Observacao,
            compra.ValorTotal, compra.DataCriacao, itens);
    }
}
