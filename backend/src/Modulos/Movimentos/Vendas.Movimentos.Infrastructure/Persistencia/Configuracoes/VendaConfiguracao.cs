using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vendas.Movimentos.Domain.Entidades;

namespace Vendas.Movimentos.Infrastructure.Persistencia.Configuracoes;

public sealed class VendaConfiguracao : IEntityTypeConfiguration<Venda>
{
    public void Configure(EntityTypeBuilder<Venda> construtor)
    {
        construtor.ToTable("Vendas");
        construtor.HasKey(v => v.Id);

        construtor.Property(v => v.Numero)
            .HasDefaultValueSql($"NEXT VALUE FOR {MovimentosDbContext.SequenciaVenda}")
            .ValueGeneratedOnAdd();

        construtor.HasIndex(v => v.Numero).IsUnique();

        construtor.Property(v => v.Data).HasColumnType("date").IsRequired();
        construtor.Property(v => v.ClienteId).IsRequired();
        construtor.Property(v => v.Observacao).HasMaxLength(Venda.TamanhoMaximoObservacao);

        // Índice sem FK: Clientes pertence ao DbContext de Cadastros (D-007). O índice serve à
        // consulta que impede excluir cliente já usado.
        construtor.HasIndex(v => v.ClienteId);
        construtor.Property(v => v.ValorTotal).HasPrecision(18, 4);
        construtor.Property(v => v.DataCriacao).IsRequired();

        construtor.Metadata
            .FindNavigation(nameof(Venda.Itens))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        construtor.HasMany(v => v.Itens)
            .WithOne()
            .HasForeignKey(i => i.VendaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class VendaItemConfiguracao : IEntityTypeConfiguration<VendaItem>
{
    public void Configure(EntityTypeBuilder<VendaItem> construtor)
    {
        construtor.ToTable("VendasItens");
        construtor.HasKey(i => i.Id);

        construtor.Property(i => i.Quantidade).HasPrecision(18, 4);
        construtor.Property(i => i.PrecoUnitario).HasPrecision(18, 4);
        construtor.Property(i => i.Subtotal).HasPrecision(18, 4);

        construtor.HasIndex(i => i.ProdutoId);
    }
}
