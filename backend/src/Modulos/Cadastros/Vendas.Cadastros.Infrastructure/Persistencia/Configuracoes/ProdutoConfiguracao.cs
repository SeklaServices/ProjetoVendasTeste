using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vendas.Cadastros.Domain.Entidades;

namespace Vendas.Cadastros.Infrastructure.Persistencia.Configuracoes;

public sealed class ProdutoConfiguracao : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> construtor)
    {
        construtor.ToTable("Produtos");
        construtor.HasKey(p => p.Id);

        construtor.Property(p => p.Codigo)
            .HasMaxLength(Produto.TamanhoMaximoCodigo)
            .IsRequired();

        // O índice único é a garantia final da unicidade do código. O handler valida antes para dar
        // uma mensagem decente; o índice impede a corrida entre duas requisições simultâneas.
        construtor.HasIndex(p => p.Codigo).IsUnique();

        construtor.Property(p => p.Nome)
            .HasMaxLength(Produto.TamanhoMaximoNome)
            .IsRequired();

        construtor.Property(p => p.UnidadeMedida)
            .HasMaxLength(Produto.TamanhoMaximoUnidade)
            .IsRequired();

        construtor.Property(p => p.PrecoCusto).HasPrecision(18, 4);
        construtor.Property(p => p.PrecoVenda).HasPrecision(18, 4);
        construtor.Property(p => p.Ativo).IsRequired();
        construtor.Property(p => p.DataCadastro).IsRequired();
    }
}
