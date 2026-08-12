using Vendas.Movimentos.Application.Dtos;
using Vendas.Movimentos.Domain.Entidades;
using Vendas.Movimentos.Domain.Interfaces;
using Vendas.Shared.Contratos;

namespace Vendas.Movimentos.Application.Queries;

public sealed record ListarVendasQuery(DateOnly? DataInicial, DateOnly? DataFinal);

public sealed class ListarVendasQueryHandler(IVendaRepositorio repositorio)
{
    public async Task<IReadOnlyList<VendaResumoDto>> HandleAsync(ListarVendasQuery consulta, CancellationToken ct)
    {
        var vendas = await repositorio.ListarAsync(consulta.DataInicial, consulta.DataFinal, ct);
        return vendas.Select(MapearResumo).ToList();
    }

    internal static VendaResumoDto MapearResumo(Venda v) =>
        new(v.Id, v.Numero, v.Data, v.Cliente, v.ValorTotal, v.Itens.Count);
}

public sealed class ObterVendaQueryHandler(
    IVendaRepositorio repositorio,
    IConsultaProdutos consultaProdutos)
{
    public async Task<VendaDetalheDto?> HandleAsync(Guid id, CancellationToken ct)
    {
        var venda = await repositorio.ObterPorIdAsync(id, ct);
        if (venda is null)
        {
            return null;
        }

        var produtos = await consultaProdutos.ObterPorIdsAsync(
            venda.Itens.Select(i => i.ProdutoId).Distinct().ToList(), ct);

        var itens = venda.Itens
            .Select(i => MapeadorDeItem.Mapear(
                i.Id, i.ProdutoId, i.Quantidade, i.PrecoUnitario, i.Subtotal, produtos))
            .ToList();

        return new VendaDetalheDto(
            venda.Id, venda.Numero, venda.Data, venda.Cliente, venda.Observacao,
            venda.ValorTotal, venda.DataCriacao, itens);
    }
}

/// <summary>Monta o item da resposta juntando os dados do documento com os do produto.</summary>
internal static class MapeadorDeItem
{
    public static ItemDto Mapear(
        Guid id,
        Guid produtoId,
        decimal quantidade,
        decimal precoUnitario,
        decimal subtotal,
        IReadOnlyList<ProdutoResumo> produtos)
    {
        var produto = produtos.FirstOrDefault(p => p.Id == produtoId);

        return new ItemDto(
            id,
            produtoId,
            produto?.Codigo ?? "?",
            produto?.Nome ?? "(produto removido)",
            produto?.UnidadeMedida ?? string.Empty,
            quantidade,
            precoUnitario,
            subtotal);
    }
}
