using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vendas.Movimentos.Domain.Entidades;

namespace Vendas.Movimentos.Infrastructure.Persistencia.Configuracoes;

public sealed class CompraConfiguracao : IEntityTypeConfiguration<Compra>
{
    public void Configure(EntityTypeBuilder<Compra> construtor)
    {
        construtor.ToTable("Compras");
        construtor.HasKey(c => c.Id);

        construtor.Property(c => c.Numero)
            .HasDefaultValueSql($"NEXT VALUE FOR {MovimentosDbContext.SequenciaCompra}")
            .ValueGeneratedOnAdd();

        construtor.HasIndex(c => c.Numero).IsUnique();

        construtor.Property(c => c.Data).HasColumnType("date").IsRequired();
        construtor.Property(c => c.Fornecedor).HasMaxLength(Compra.TamanhoMaximoParceiro).IsRequired();
        construtor.Property(c => c.Observacao).HasMaxLength(Compra.TamanhoMaximoObservacao);
        construtor.Property(c => c.ValorTotal).HasPrecision(18, 4);
        construtor.Property(c => c.DataCriacao).IsRequired();

        // A coleção é privada na entidade (só se altera pelo construtor). O EF acessa pelo campo.
        construtor.Metadata
            .FindNavigation(nameof(Compra.Itens))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        construtor.HasMany(c => c.Itens)
            .WithOne()
            .HasForeignKey(i => i.CompraId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class CompraItemConfiguracao : IEntityTypeConfiguration<CompraItem>
{
    public void Configure(EntityTypeBuilder<CompraItem> construtor)
    {
        construtor.ToTable("ComprasItens");
        construtor.HasKey(i => i.Id);

        construtor.Property(i => i.Quantidade).HasPrecision(18, 4);
        construtor.Property(i => i.PrecoUnitario).HasPrecision(18, 4);
        construtor.Property(i => i.Subtotal).HasPrecision(18, 4);

        // Índice em ProdutoId (e não FK): a tabela Produtos pertence a OUTRO módulo, e uma FK entre
        // DbContexts diferentes tornaria as migrations dependentes uma da outra. A integridade é
        // garantida na camada Application (ExcluirProdutoCommandHandler + validação dos itens).
        construtor.HasIndex(i => i.ProdutoId);
    }
}
