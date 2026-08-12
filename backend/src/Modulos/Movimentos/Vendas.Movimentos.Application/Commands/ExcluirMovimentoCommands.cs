using Vendas.Movimentos.Domain.Interfaces;

namespace Vendas.Movimentos.Application.Commands;

public sealed class ExcluirCompraCommandHandler(ICompraRepositorio repositorio)
{
    /// <returns><c>false</c> quando não existe — o endpoint traduz em 404.</returns>
    public async Task<bool> HandleAsync(Guid id, CancellationToken ct)
    {
        var compra = await repositorio.ObterPorIdAsync(id, ct);
        if (compra is null)
        {
            return false;
        }

        repositorio.Remover(compra);
        await repositorio.SalvarAsync(ct);
        return true;
    }
}

public sealed class ExcluirVendaCommandHandler(IVendaRepositorio repositorio)
{
    public async Task<bool> HandleAsync(Guid id, CancellationToken ct)
    {
        var venda = await repositorio.ObterPorIdAsync(id, ct);
        if (venda is null)
        {
            return false;
        }

        repositorio.Remover(venda);
        await repositorio.SalvarAsync(ct);
        return true;
    }
}
