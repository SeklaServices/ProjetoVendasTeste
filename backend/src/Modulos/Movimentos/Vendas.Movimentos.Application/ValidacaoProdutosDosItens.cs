using Vendas.Shared.Contratos;
using Vendas.Shared.Excecoes;

namespace Vendas.Movimentos.Application;

/// <summary>
/// Regra de caso de uso compartilhada por compras e vendas: todo item precisa apontar para um
/// produto que existe e está ativo.
///
/// Não cabe na entidade — a entidade não pode consultar o catálogo. Fica aqui, na camada
/// Application, e nunca no endpoint.
/// </summary>
internal static class ValidacaoProdutosDosItens
{
    public static async Task GarantirProdutosValidosAsync(
        IConsultaProdutos consultaProdutos, IReadOnlyCollection<Guid> produtoIds, CancellationToken ct)
    {
        var distintos = produtoIds.Distinct().ToList();
        var encontrados = await consultaProdutos.ObterPorIdsAsync(distintos, ct);

        var inexistente = distintos.FirstOrDefault(id => encontrados.All(p => p.Id != id));
        if (inexistente != Guid.Empty)
        {
            throw new RegraDeNegocioExcecao(
                "ITEM_PRODUTO_INEXISTENTE", $"O produto {inexistente} não existe.");
        }

        var inativo = encontrados.FirstOrDefault(p => !p.Ativo);
        if (inativo is not null)
        {
            throw new RegraDeNegocioExcecao(
                "ITEM_PRODUTO_INATIVO",
                $"O produto '{inativo.Codigo} — {inativo.Nome}' está inativo e não pode ser movimentado.");
        }
    }
}
