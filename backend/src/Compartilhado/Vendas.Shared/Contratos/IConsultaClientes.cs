namespace Vendas.Shared.Contratos;

/// <summary>
/// Como o módulo Movimentos enxerga os clientes.
///
/// Mesmo padrão de <see cref="IConsultaProdutos"/>: Movimentos precisa validar o cliente de uma
/// venda, mas não pode acessar o DbContext de Cadastros. Cadastros implementa este contrato, e
/// Movimentos depende só da interface.
/// </summary>
public interface IConsultaClientes
{
    /// <summary>Clientes existentes entre os ids pedidos. Ids inexistentes não voltam.</summary>
    Task<IReadOnlyList<ClienteResumo>> ObterPorIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct);
}

/// <summary>O mínimo que Movimentos precisa saber sobre um cliente. Não é a entidade Cliente.</summary>
public sealed record ClienteResumo(Guid Id, int Codigo, string Nome, bool Ativo);
