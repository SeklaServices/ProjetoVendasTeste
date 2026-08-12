using Microsoft.EntityFrameworkCore;
using Vendas.Cadastros.Domain.Entidades;
using Vendas.Cadastros.Domain.Interfaces;

namespace Vendas.Cadastros.Infrastructure.Persistencia.Repositorios;

/// <summary>
/// Só persistência. Nenhuma regra de negócio mora aqui — se aparecer um <c>if</c> decidindo se algo
/// é permitido, ele está no lugar errado e o review deve barrar.
/// </summary>
public sealed class ProdutoRepositorio(CadastrosDbContext contexto) : IProdutoRepositorio
{
    public Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken ct)
        => contexto.Produtos.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<bool> ExisteCodigoAsync(string codigo, Guid? idIgnorado, CancellationToken ct)
        => contexto.Produtos.AnyAsync(
            p => p.Codigo == codigo && (idIgnorado == null || p.Id != idIgnorado), ct);

    public async Task<IReadOnlyList<Produto>> ListarAsync(string? busca, bool apenasAtivos, CancellationToken ct)
    {
        var consulta = contexto.Produtos.AsNoTracking().AsQueryable();

        if (apenasAtivos)
        {
            consulta = consulta.Where(p => p.Ativo);
        }

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            consulta = consulta.Where(p => p.Codigo.Contains(termo) || p.Nome.Contains(termo));
        }

        return await consulta.OrderBy(p => p.Nome).ToListAsync(ct);
    }

    public async Task AdicionarAsync(Produto produto, CancellationToken ct)
        => await contexto.Produtos.AddAsync(produto, ct);

    public void Remover(Produto produto) => contexto.Produtos.Remove(produto);

    public Task SalvarAsync(CancellationToken ct) => contexto.SaveChangesAsync(ct);
}
