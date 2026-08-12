using NSubstitute;
using Vendas.Movimentos.Application.Commands;
using Vendas.Movimentos.Domain.Entidades;
using Vendas.Movimentos.Domain.Interfaces;
using Vendas.Shared.Contratos;
using Vendas.Shared.Excecoes;
using Xunit;

namespace Vendas.Movimentos.Tests.Unit;

public class CriarCompraCommandHandlerTestes
{
    private static readonly DateOnly Hoje = DateOnly.FromDateTime(DateTime.Today);

    private readonly ICompraRepositorio _repositorio = Substitute.For<ICompraRepositorio>();
    private readonly IConsultaProdutos _produtos = Substitute.For<IConsultaProdutos>();

    private CriarCompraCommandHandler CriarHandler() => new(_repositorio, _produtos);

    private void ConfigurarProduto(Guid id, bool ativo) =>
        _produtos.ObterPorIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([new ProdutoResumo(id, "ABC", "Banana", "KG", ativo)]);

    [Fact]
    public async Task Compra_com_produto_ativo_e_persistida()
    {
        var produtoId = Guid.NewGuid();
        ConfigurarProduto(produtoId, ativo: true);

        await CriarHandler().HandleAsync(
            new CriarCompraCommand(Hoje, "Fornecedor X", null, [new ItemCommand(produtoId, 2m, 10m)]),
            default);

        await _repositorio.Received(1).AdicionarAsync(Arg.Any<Compra>(), Arg.Any<CancellationToken>());
        await _repositorio.Received(1).SalvarAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Produto_inativo_e_rejeitado()
    {
        var produtoId = Guid.NewGuid();
        ConfigurarProduto(produtoId, ativo: false);

        var erro = await Assert.ThrowsAsync<RegraDeNegocioExcecao>(() =>
            CriarHandler().HandleAsync(
                new CriarCompraCommand(Hoje, "Fornecedor X", null, [new ItemCommand(produtoId, 2m, 10m)]),
                default));

        Assert.Equal("ITEM_PRODUTO_INATIVO", erro.Codigo);
        await _repositorio.DidNotReceive().AdicionarAsync(Arg.Any<Compra>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Produto_inexistente_e_rejeitado()
    {
        _produtos.ObterPorIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var erro = await Assert.ThrowsAsync<RegraDeNegocioExcecao>(() =>
            CriarHandler().HandleAsync(
                new CriarCompraCommand(Hoje, "Fornecedor X", null, [new ItemCommand(Guid.NewGuid(), 2m, 10m)]),
                default));

        Assert.Equal("ITEM_PRODUTO_INEXISTENTE", erro.Codigo);
    }

    [Fact]
    public async Task Compra_sem_itens_e_rejeitada_antes_de_tocar_o_repositorio()
    {
        _produtos.ObterPorIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var erro = await Assert.ThrowsAsync<RegraDeNegocioExcecao>(() =>
            CriarHandler().HandleAsync(new CriarCompraCommand(Hoje, "Fornecedor X", null, []), default));

        Assert.Equal("COMPRA_SEM_ITENS", erro.Codigo);
        await _repositorio.DidNotReceive().SalvarAsync(Arg.Any<CancellationToken>());
    }
}
