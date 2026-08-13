using Vendas.Movimentos.Domain.Entidades;

namespace Vendas.Movimentos.Domain.Interfaces;

public interface ICompraRepositorio
{
    Task<Compra?> ObterPorIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Compra>> ListarAsync(DateOnly? dataInicial, DateOnly? dataFinal, CancellationToken ct);
    Task AdicionarAsync(Compra compra, CancellationToken ct);
    void Remover(Compra compra);
    Task SalvarAsync(CancellationToken ct);
}

public interface IVendaRepositorio
{
    Task<Venda?> ObterPorIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Venda>> ListarAsync(DateOnly? dataInicial, DateOnly? dataFinal, CancellationToken ct);
    Task AdicionarAsync(Venda venda, CancellationToken ct);
    void Remover(Venda venda);
    Task SalvarAsync(CancellationToken ct);
}
