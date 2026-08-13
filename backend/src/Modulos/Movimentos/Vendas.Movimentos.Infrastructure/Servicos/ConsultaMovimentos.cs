using Microsoft.EntityFrameworkCore;
using Vendas.Movimentos.Infrastructure.Persistencia;
using Vendas.Shared.Contratos;

namespace Vendas.Movimentos.Infrastructure.Servicos;

/// <summary>
/// O que Cadastros precisa saber sobre movimentos: se um produto já foi usado. Sem isto, Cadastros
/// teria que consultar as tabelas de Movimentos diretamente — o que é proibido.
/// </summary>
public sealed class ConsultaMovimentos(MovimentosDbContext contexto) : IConsultaMovimentos
{
    public async Task<bool> ProdutoUtilizadoAsync(Guid produtoId, CancellationToken ct)
        => await contexto.Compras.AnyAsync(c => c.Itens.Any(i => i.ProdutoId == produtoId), ct)
        || await contexto.Vendas.AnyAsync(v => v.Itens.Any(i => i.ProdutoId == produtoId), ct);

    public Task<bool> ClienteUtilizadoAsync(Guid clienteId, CancellationToken ct)
        => contexto.Vendas.AnyAsync(v => v.ClienteId == clienteId, ct);
}
