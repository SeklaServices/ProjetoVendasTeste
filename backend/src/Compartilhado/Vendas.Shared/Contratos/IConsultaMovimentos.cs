namespace Vendas.Shared.Contratos;

/// <summary>
/// O caminho inverso de <see cref="IConsultaProdutos"/>: Cadastros precisa saber se um registro já
/// foi usado em alguma compra ou venda antes de permitir a exclusão. Implementado em Movimentos.
/// </summary>
public interface IConsultaMovimentos
{
    Task<bool> ProdutoUtilizadoAsync(Guid produtoId, CancellationToken ct);

    /// <summary>
    /// Se alguma venda aponta para este cliente. Não existe FK entre os módulos (D-007), então é
    /// esta consulta que impede excluir um cliente com histórico.
    /// </summary>
    Task<bool> ClienteUtilizadoAsync(Guid clienteId, CancellationToken ct);
}
