using Microsoft.EntityFrameworkCore;
using Vendas.Movimentos.Domain.Entidades;
using Vendas.Movimentos.Domain.Interfaces;

namespace Vendas.Movimentos.Infrastructure.Persistencia.Repositorios;

public sealed class CompraRepositorio(MovimentosDbContext contexto) : ICompraRepositorio
{
    public Task<Compra?> ObterPorIdAsync(Guid id, CancellationToken ct)
        => contexto.Compras.Include(c => c.Itens).FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Compra>> ListarAsync(
        DateOnly? dataInicial, DateOnly? dataFinal, CancellationToken ct)
    {
        var consulta = contexto.Compras.AsNoTracking().Include(c => c.Itens).AsQueryable();

        if (dataInicial is not null)
        {
            consulta = consulta.Where(c => c.Data >= dataInicial);
        }

        if (dataFinal is not null)
        {
            consulta = consulta.Where(c => c.Data <= dataFinal);
        }

        return await consulta.OrderByDescending(c => c.Numero).ToListAsync(ct);
    }

    public async Task AdicionarAsync(Compra compra, CancellationToken ct)
        => await contexto.Compras.AddAsync(compra, ct);

    public void Remover(Compra compra) => contexto.Compras.Remove(compra);

    public Task SalvarAsync(CancellationToken ct) => contexto.SaveChangesAsync(ct);
}

public sealed class VendaRepositorio(MovimentosDbContext contexto) : IVendaRepositorio
{
    public Task<Venda?> ObterPorIdAsync(Guid id, CancellationToken ct)
        => contexto.Vendas.Include(v => v.Itens).FirstOrDefaultAsync(v => v.Id == id, ct);

    public async Task<IReadOnlyList<Venda>> ListarAsync(
        DateOnly? dataInicial, DateOnly? dataFinal, CancellationToken ct)
    {
        var consulta = contexto.Vendas.AsNoTracking().Include(v => v.Itens).AsQueryable();

        if (dataInicial is not null)
        {
            consulta = consulta.Where(v => v.Data >= dataInicial);
        }

        if (dataFinal is not null)
        {
            consulta = consulta.Where(v => v.Data <= dataFinal);
        }

        return await consulta.OrderByDescending(v => v.Numero).ToListAsync(ct);
    }

    public async Task AdicionarAsync(Venda venda, CancellationToken ct)
        => await contexto.Vendas.AddAsync(venda, ct);

    public void Remover(Venda venda) => contexto.Vendas.Remove(venda);

    public Task SalvarAsync(CancellationToken ct) => contexto.SaveChangesAsync(ct);
}
