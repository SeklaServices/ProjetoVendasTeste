using Microsoft.EntityFrameworkCore;
using Vendas.Movimentos.Domain.Entidades;

namespace Vendas.Movimentos.Infrastructure.Persistencia;

public sealed class MovimentosDbContext(DbContextOptions<MovimentosDbContext> opcoes) : DbContext(opcoes)
{
    public const string SequenciaCompra = "SeqCompra";
    public const string SequenciaVenda = "SeqVenda";

    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<Venda> Vendas => Set<Venda>();

    protected override void OnModelCreating(ModelBuilder modelo)
    {
        // Numeração sequencial pelo banco: garante unicidade mesmo com duas requisições ao mesmo
        // tempo, o que um MAX(Numero)+1 na aplicação não garante.
        modelo.HasSequence<int>(SequenciaCompra).StartsAt(1).IncrementsBy(1);
        modelo.HasSequence<int>(SequenciaVenda).StartsAt(1).IncrementsBy(1);

        modelo.ApplyConfigurationsFromAssembly(typeof(MovimentosDbContext).Assembly);
        base.OnModelCreating(modelo);
    }
}
