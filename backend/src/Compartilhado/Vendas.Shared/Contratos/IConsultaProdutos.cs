namespace Vendas.Shared.Contratos;

/// <summary>
/// Como o módulo Movimentos enxerga os produtos.
///
/// Movimentos precisa saber se um produto existe e está ativo, mas NÃO pode acessar o DbContext de
/// Cadastros (regra do projeto). Então Cadastros implementa este contrato, e Movimentos depende só
/// da interface — que vive aqui, no projeto compartilhado, e não conhece nenhum dos dois módulos.
/// </summary>
public interface IConsultaProdutos
{
    /// <summary>Produtos existentes entre os ids pedidos. Ids inexistentes simplesmente não voltam.</summary>
    Task<IReadOnlyList<ProdutoResumo>> ObterPorIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct);

    /// <summary>Quantos produtos existem. Usado pela tela de resumo.</summary>
    Task<(int Total, int Ativos)> ContarAsync(CancellationToken ct);
}

/// <summary>O mínimo que Movimentos precisa saber sobre um produto. Não é a entidade Produto.</summary>
public sealed record ProdutoResumo(Guid Id, string Codigo, string Nome, string UnidadeMedida, bool Ativo);
