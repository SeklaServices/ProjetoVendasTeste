using Microsoft.EntityFrameworkCore;
using Vendas.Cadastros.Domain.Entidades;

namespace Vendas.Cadastros.Infrastructure.Persistencia;

/// <summary>
/// DbContext do módulo Cadastros. Conhece apenas as tabelas deste módulo.
/// O módulo Movimentos tem o seu, no mesmo banco físico — e um nunca enxerga o outro.
/// </summary>
public sealed class CadastrosDbContext(DbContextOptions<CadastrosDbContext> opcoes) : DbContext(opcoes)
{
    public const string SequenciaCliente = "SeqCliente";

    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Cliente> Clientes => Set<Cliente>();

    protected override void OnModelCreating(ModelBuilder modelo)
    {
        // Numeração sequencial pelo banco, como em Compra e Venda: garante unicidade mesmo com
        // duas requisições ao mesmo tempo, o que um MAX(Codigo)+1 na aplicação não garante.
        modelo.HasSequence<int>(SequenciaCliente).StartsAt(1).IncrementsBy(1);

        modelo.ApplyConfigurationsFromAssembly(typeof(CadastrosDbContext).Assembly);
        base.OnModelCreating(modelo);
    }
}
