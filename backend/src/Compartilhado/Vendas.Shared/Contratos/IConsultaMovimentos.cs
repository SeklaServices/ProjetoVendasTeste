namespace Vendas.Shared.Contratos;

/// <summary>
/// O caminho inverso de <see cref="IConsultaProdutos"/>: Cadastros precisa saber se um produto já
/// foi usado em alguma compra ou venda antes de permitir a exclusão. Implementado em Movimentos.
/// </summary>
public interface IConsultaMovimentos
{
    Task<bool> ProdutoUtilizadoAsync(Guid produtoId, CancellationToken ct);
}
