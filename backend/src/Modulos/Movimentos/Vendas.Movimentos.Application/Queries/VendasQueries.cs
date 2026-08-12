using Vendas.Movimentos.Application.Dtos;
using Vendas.Movimentos.Domain.Entidades;
using Vendas.Movimentos.Domain.Interfaces;
using Vendas.Shared.Contratos;

namespace Vendas.Movimentos.Application.Queries;

public sealed record ListarVendasQuery(DateOnly? DataInicial, DateOnly? DataFinal);

public sealed class ListarVendasQueryHandler(
    IVendaRepositorio repositorio,
    IConsultaClientes consultaClientes)
{
    public async Task<IReadOnlyList<VendaResumoDto>> HandleAsync(ListarVendasQuery consulta, CancellationToken ct)
    {
        var vendas = await repositorio.ListarAsync(consulta.DataInicial, consulta.DataFinal, ct);
        var clientes = await MapeadorDeVenda.ObterClientesAsync(consultaClientes, vendas, ct);

        return vendas.Select(v => MapeadorDeVenda.MapearResumo(v, clientes)).ToList();
    }
}

public sealed class ObterVendaQueryHandler(
    IVendaRepositorio repositorio,
    IConsultaProdutos consultaProdutos,
    IConsultaClientes consultaClientes)
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

        var clientes = await MapeadorDeVenda.ObterClientesAsync(consultaClientes, [venda], ct);
        var cliente = MapeadorDeVenda.Resolver(venda.ClienteId, clientes);

        var itens = venda.Itens
            .Select(i => MapeadorDeItem.Mapear(
                i.Id, i.ProdutoId, i.Quantidade, i.PrecoUnitario, i.Subtotal, produtos))
            .ToList();

        return new VendaDetalheDto(
            venda.Id, venda.Numero, venda.Data,
            venda.ClienteId, cliente.Codigo, cliente.Nome,
            venda.Observacao, venda.ValorTotal, venda.DataCriacao, itens);
    }
}

/// <summary>
/// Junta a venda com os dados do cliente, que vivem no módulo Cadastros.
///
/// Uma consulta só para todas as vendas da lista — resolver cliente por cliente dentro do laço
/// seria uma ida ao banco por linha da grid.
/// </summary>
internal static class MapeadorDeVenda
{
    public static async Task<IReadOnlyList<ClienteResumo>> ObterClientesAsync(
        IConsultaClientes consultaClientes, IReadOnlyCollection<Venda> vendas, CancellationToken ct)
        => await consultaClientes.ObterPorIdsAsync(
            vendas.Select(v => v.ClienteId).Distinct().ToList(), ct);

    public static ClienteResumo Resolver(Guid clienteId, IReadOnlyList<ClienteResumo> clientes)
        => clientes.FirstOrDefault(c => c.Id == clienteId)
           ?? new ClienteResumo(clienteId, 0, "(cliente removido)", false);

    public static VendaResumoDto MapearResumo(Venda v, IReadOnlyList<ClienteResumo> clientes)
    {
        var cliente = Resolver(v.ClienteId, clientes);
        return new VendaResumoDto(
            v.Id, v.Numero, v.Data, v.ClienteId, cliente.Codigo, cliente.Nome, v.ValorTotal, v.Itens.Count);
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
