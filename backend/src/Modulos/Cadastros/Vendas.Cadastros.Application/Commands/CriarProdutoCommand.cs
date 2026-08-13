using Vendas.Cadastros.Domain.Entidades;
using Vendas.Cadastros.Domain.Interfaces;
using Vendas.Shared.Excecoes;

namespace Vendas.Cadastros.Application.Commands;

public sealed record CriarProdutoCommand(
    string Codigo,
    string Nome,
    string UnidadeMedida,
    decimal PrecoCusto,
    decimal PrecoVenda);

public sealed class CriarProdutoCommandHandler(IProdutoRepositorio repositorio)
{
    public async Task<Guid> HandleAsync(CriarProdutoCommand comando, CancellationToken ct)
    {
        var codigo = (comando.Codigo ?? string.Empty).Trim();

        // Unicidade é regra de caso de uso, não de entidade: a entidade não enxerga as outras.
        if (await repositorio.ExisteCodigoAsync(codigo, idIgnorado: null, ct))
        {
            throw new RegraDeNegocioExcecao(
                "PRODUTO_CODIGO_DUPLICADO", $"Já existe um produto com o código '{codigo}'.");
        }

        var produto = Produto.Criar(
            codigo, comando.Nome, comando.UnidadeMedida, comando.PrecoCusto, comando.PrecoVenda);

        await repositorio.AdicionarAsync(produto, ct);
        await repositorio.SalvarAsync(ct);

        return produto.Id;
    }
}
