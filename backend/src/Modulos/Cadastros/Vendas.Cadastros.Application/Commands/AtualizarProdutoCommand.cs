using Vendas.Cadastros.Domain.Interfaces;
using Vendas.Shared.Excecoes;

namespace Vendas.Cadastros.Application.Commands;

public sealed record AtualizarProdutoCommand(
    Guid Id,
    string Codigo,
    string Nome,
    string UnidadeMedida,
    decimal PrecoCusto,
    decimal PrecoVenda,
    bool Ativo);

public sealed class AtualizarProdutoCommandHandler(IProdutoRepositorio repositorio)
{
    /// <returns><c>false</c> quando o produto não existe — o endpoint traduz em 404.</returns>
    public async Task<bool> HandleAsync(AtualizarProdutoCommand comando, CancellationToken ct)
    {
        var produto = await repositorio.ObterPorIdAsync(comando.Id, ct);
        if (produto is null)
        {
            return false;
        }

        var codigo = (comando.Codigo ?? string.Empty).Trim();

        if (await repositorio.ExisteCodigoAsync(codigo, idIgnorado: comando.Id, ct))
        {
            throw new RegraDeNegocioExcecao(
                "PRODUTO_CODIGO_DUPLICADO", $"Já existe um produto com o código '{codigo}'.");
        }

        produto.Atualizar(
            codigo, comando.Nome, comando.UnidadeMedida, comando.PrecoCusto, comando.PrecoVenda, comando.Ativo);

        await repositorio.SalvarAsync(ct);
        return true;
    }
}
