using Vendas.Movimentos.Domain.Entidades;
using Vendas.Movimentos.Domain.Interfaces;
using Vendas.Shared.Contratos;

namespace Vendas.Movimentos.Application.Commands;

public sealed record ItemCommand(Guid ProdutoId, decimal Quantidade, decimal PrecoUnitario);

public sealed record CriarCompraCommand(
    DateOnly Data,
    string Fornecedor,
    string? Observacao,
    IReadOnlyList<ItemCommand> Itens);

public sealed class CriarCompraCommandHandler(
    ICompraRepositorio repositorio,
    IConsultaProdutos consultaProdutos)
{
    public async Task<(Guid Id, int Numero)> HandleAsync(CriarCompraCommand comando, CancellationToken ct)
    {
        var itensInformados = comando.Itens ?? [];

        await ValidacaoProdutosDosItens.GarantirProdutosValidosAsync(
            consultaProdutos, itensInformados.Select(i => i.ProdutoId).ToList(), ct);

        var itens = itensInformados
            .Select(i => CompraItem.Criar(i.ProdutoId, i.Quantidade, i.PrecoUnitario))
            .ToList();

        var compra = Compra.Criar(comando.Data, comando.Fornecedor, comando.Observacao, itens);

        await repositorio.AdicionarAsync(compra, ct);
        await repositorio.SalvarAsync(ct);

        // Numero só existe depois do SaveChanges: quem gera é a SEQUENCE do banco.
        return (compra.Id, compra.Numero);
    }
}
