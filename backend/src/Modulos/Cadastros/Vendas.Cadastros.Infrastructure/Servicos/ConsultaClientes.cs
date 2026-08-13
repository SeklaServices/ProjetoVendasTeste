using Microsoft.EntityFrameworkCore;
using Vendas.Cadastros.Infrastructure.Persistencia;
using Vendas.Shared.Contratos;

namespace Vendas.Cadastros.Infrastructure.Servicos;

/// <summary>
/// Implementação, do lado de Cadastros, do contrato que Movimentos usa para enxergar clientes.
/// Movimentos depende da interface em Vendas.Shared e nunca deste tipo.
/// </summary>
public sealed class ConsultaClientes(CadastrosDbContext contexto) : IConsultaClientes
{
    public async Task<IReadOnlyList<ClienteResumo>> ObterPorIdsAsync(
        IReadOnlyCollection<Guid> ids, CancellationToken ct)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        return await contexto.Clientes
            .AsNoTracking()
            .Where(c => ids.Contains(c.Id))
            .Select(c => new ClienteResumo(c.Id, c.Codigo, c.Nome, c.Ativo))
            .ToListAsync(ct);
    }
}
