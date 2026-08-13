using Vendas.Movimentos.Application.Dtos;
using Vendas.Movimentos.Domain.Interfaces;
using Vendas.Shared.Contratos;

namespace Vendas.Movimentos.Application.Queries;

public sealed record ResumoQuery(DateOnly? DataInicial, DateOnly? DataFinal);

public sealed class ResumoQueryHandler(
    ICompraRepositorio compras,
    IVendaRepositorio vendas,
    IConsultaProdutos consultaProdutos,
    IConsultaClientes consultaClientes)
{
    private const int QuantidadeUltimosMovimentos = 5;

    public async Task<ResumoDto> HandleAsync(ResumoQuery consulta, CancellationToken ct)
    {
        var listaCompras = await compras.ListarAsync(consulta.DataInicial, consulta.DataFinal, ct);
        var listaVendas = await vendas.ListarAsync(consulta.DataInicial, consulta.DataFinal, ct);
        var (totalProdutos, produtosAtivos) = await consultaProdutos.ContarAsync(ct);
        var clientes = await MapeadorDeVenda.ObterClientesAsync(consultaClientes, listaVendas, ct);

        var totalComprado = listaCompras.Sum(c => c.ValorTotal);
        var totalVendido = listaVendas.Sum(v => v.ValorTotal);

        return new ResumoDto(
            TotalComprado: totalComprado,
            TotalVendido: totalVendido,
            Diferenca: totalVendido - totalComprado,
            QuantidadeCompras: listaCompras.Count,
            QuantidadeVendas: listaVendas.Count,
            ProdutosCadastrados: totalProdutos,
            ProdutosAtivos: produtosAtivos,
            UltimasCompras: listaCompras
                .OrderByDescending(c => c.Numero)
                .Take(QuantidadeUltimosMovimentos)
                .Select(ListarComprasQueryHandler.MapearResumo)
                .ToList(),
            UltimasVendas: listaVendas
                .OrderByDescending(v => v.Numero)
                .Take(QuantidadeUltimosMovimentos)
                .Select(v => MapeadorDeVenda.MapearResumo(v, clientes))
                .ToList());
    }
}
