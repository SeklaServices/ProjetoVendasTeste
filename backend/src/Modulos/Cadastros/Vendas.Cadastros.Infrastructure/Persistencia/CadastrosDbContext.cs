using Microsoft.EntityFrameworkCore;
using Vendas.Cadastros.Domain.Entidades;

namespace Vendas.Cadastros.Infrastructure.Persistencia;

/// <summary>
/// DbContext do módulo Cadastros. Conhece apenas as tabelas deste módulo.
/// O módulo Movimentos tem o seu, no mesmo banco físico — e um nunca enxerga o outro.
/// </summary>
public sealed class CadastrosDbContext(DbContextOptions<CadastrosDbContext> opcoes) : DbContext(opcoes)
{
    public DbSet<Produto> Produtos => Set<Produto>();

    protected override void OnModelCreating(ModelBuilder modelo)
    {
        modelo.ApplyConfigurationsFromAssembly(typeof(CadastrosDbContext).Assembly);
        base.OnModelCreating(modelo);
    }
}
