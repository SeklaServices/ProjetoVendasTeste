using Vendas.Cadastros.Domain.Interfaces;
using Vendas.Shared.Contratos;
using Vendas.Shared.Excecoes;

namespace Vendas.Cadastros.Application.Commands;

public sealed class ExcluirProdutoCommandHandler(
    IProdutoRepositorio repositorio,
    IConsultaMovimentos consultaMovimentos)
{
    /// <returns><c>false</c> quando o produto não existe — o endpoint traduz em 404.</returns>
    public async Task<bool> HandleAsync(Guid id, CancellationToken ct)
    {
        var produto = await repositorio.ObterPorIdAsync(id, ct);
        if (produto is null)
        {
            return false;
        }

        // A FK do banco também impede (RESTRICT), mas confiar só nela devolveria um erro de
        // infraestrutura sem mensagem útil. Aqui o usuário recebe a orientação certa: inative.
        if (await consultaMovimentos.ProdutoUtilizadoAsync(id, ct))
        {
            throw new RegraDeNegocioExcecao(
                "PRODUTO_EM_USO",
                "Este produto já foi usado em uma compra ou venda e não pode ser excluído. Inative-o.");
        }

        repositorio.Remover(produto);
        await repositorio.SalvarAsync(ct);
        return true;
    }
}
