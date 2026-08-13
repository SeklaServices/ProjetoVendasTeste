using Microsoft.EntityFrameworkCore;
using Vendas.Cadastros.Domain.Entidades;
using Vendas.Cadastros.Domain.Interfaces;

namespace Vendas.Cadastros.Infrastructure.Persistencia.Repositorios;

public sealed class ClienteRepositorio(CadastrosDbContext contexto) : IClienteRepositorio
{
    public Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken ct)
        => contexto.Clientes.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> ExisteDocumentoAsync(string documento, Guid? idIgnorado, CancellationToken ct)
        => contexto.Clientes.AnyAsync(
            c => c.Documento == documento && (idIgnorado == null || c.Id != idIgnorado), ct);

    public async Task<IReadOnlyList<Cliente>> ListarAsync(string? busca, bool apenasAtivos, CancellationToken ct)
    {
        var consulta = contexto.Clientes.AsNoTracking().AsQueryable();

        if (apenasAtivos)
        {
            consulta = consulta.Where(c => c.Ativo);
        }

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();

            // Busca por número do código também: quem lança a venda decora o código, não o Guid.
            var codigo = int.TryParse(termo, out var numero) ? numero : (int?)null;

            consulta = consulta.Where(c =>
                c.Nome.Contains(termo)
                || (c.Documento != null && c.Documento.Contains(termo))
                || (codigo != null && c.Codigo == codigo));
        }

        return await consulta.OrderBy(c => c.Nome).ToListAsync(ct);
    }

    public async Task AdicionarAsync(Cliente cliente, CancellationToken ct)
        => await contexto.Clientes.AddAsync(cliente, ct);

    public void Remover(Cliente cliente) => contexto.Clientes.Remove(cliente);

    public Task SalvarAsync(CancellationToken ct) => contexto.SaveChangesAsync(ct);
}
