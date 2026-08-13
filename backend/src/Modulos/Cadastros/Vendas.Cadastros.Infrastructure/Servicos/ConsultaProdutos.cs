using Microsoft.EntityFrameworkCore;
using Vendas.Cadastros.Infrastructure.Persistencia;
using Vendas.Shared.Contratos;

namespace Vendas.Cadastros.Infrastructure.Servicos;

/// <summary>
/// Implementação, do lado de Cadastros, do contrato que Movimentos usa para enxergar produtos.
/// Movimentos depende da interface em Vendas.Shared e nunca deste tipo — é essa indireção que
/// mantém os módulos independentes.
/// </summary>
public sealed class ConsultaProdutos(CadastrosDbContext contexto) : IConsultaProdutos
{
    public async Task<IReadOnlyList<ProdutoResumo>> ObterPorIdsAsync(
        IReadOnlyCollection<Guid> ids, CancellationToken ct)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        return await contexto.Produtos
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .Select(p => new ProdutoResumo(p.Id, p.Codigo, p.Nome, p.UnidadeMedida, p.Ativo))
            .ToListAsync(ct);
    }

    public async Task<(int Total, int Ativos)> ContarAsync(CancellationToken ct)
    {
        var total = await contexto.Produtos.CountAsync(ct);
        var ativos = await contexto.Produtos.CountAsync(p => p.Ativo, ct);
        return (total, ativos);
    }
}
