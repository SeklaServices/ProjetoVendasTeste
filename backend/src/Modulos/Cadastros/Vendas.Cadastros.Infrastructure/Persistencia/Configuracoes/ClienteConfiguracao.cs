using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vendas.Cadastros.Domain.Entidades;

namespace Vendas.Cadastros.Infrastructure.Persistencia.Configuracoes;

public sealed class ClienteConfiguracao : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> construtor)
    {
        construtor.ToTable("Clientes");
        construtor.HasKey(c => c.Id);

        construtor.Property(c => c.Codigo)
            .HasDefaultValueSql($"NEXT VALUE FOR {CadastrosDbContext.SequenciaCliente}")
            .ValueGeneratedOnAdd();

        construtor.HasIndex(c => c.Codigo).IsUnique();

        construtor.Property(c => c.Nome).HasMaxLength(Cliente.TamanhoMaximoNome).IsRequired();
        construtor.Property(c => c.Documento).HasMaxLength(Cliente.TamanhoMaximoDocumento);
        construtor.Property(c => c.Telefone).HasMaxLength(Cliente.TamanhoMaximoTelefone);
        construtor.Property(c => c.Email).HasMaxLength(Cliente.TamanhoMaximoEmail);
        construtor.Property(c => c.Ativo).IsRequired();
        construtor.Property(c => c.DataCadastro).IsRequired();

        // Índice único FILTRADO: o documento é opcional, e no SQL Server um índice único comum
        // aceitaria apenas UM registro com NULL. Com o filtro, N clientes sem documento convivem e
        // a unicidade vale só para os que têm.
        construtor.HasIndex(c => c.Documento)
            .IsUnique()
            .HasFilter("[Documento] IS NOT NULL");

        construtor.HasIndex(c => c.Nome);
    }
}
