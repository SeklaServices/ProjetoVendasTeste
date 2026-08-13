using NSubstitute;
using Vendas.Cadastros.Application.Commands;
using Vendas.Cadastros.Domain.Entidades;
using Vendas.Cadastros.Domain.Interfaces;
using Vendas.Shared.Contratos;
using Vendas.Shared.Excecoes;
using Xunit;

namespace Vendas.Cadastros.Tests.Unit;

/// <summary>
/// Regras de caso de uso — as que a entidade não consegue verificar sozinha porque dependem do
/// que já existe no repositório. O repositório é substituído; nunca há banco em teste unitário.
/// </summary>
public class CriarProdutoCommandHandlerTestes
{
    private readonly IProdutoRepositorio _repositorio = Substitute.For<IProdutoRepositorio>();

    [Fact]
    public async Task Codigo_duplicado_e_rejeitado()
    {
        _repositorio.ExisteCodigoAsync("ABC", null, Arg.Any<CancellationToken>()).Returns(true);
        var handler = new CriarProdutoCommandHandler(_repositorio);

        var erro = await Assert.ThrowsAsync<RegraDeNegocioExcecao>(() =>
            handler.HandleAsync(new CriarProdutoCommand("ABC", "Banana", "UN", 1m, 2m), default));

        Assert.Equal("PRODUTO_CODIGO_DUPLICADO", erro.Codigo);
        await _repositorio.DidNotReceive().AdicionarAsync(Arg.Any<Produto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Produto_valido_e_persistido()
    {
        _repositorio.ExisteCodigoAsync(Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(false);
        var handler = new CriarProdutoCommandHandler(_repositorio);

        var id = await handler.HandleAsync(new CriarProdutoCommand("ABC", "Banana", "UN", 1m, 2m), default);

        Assert.NotEqual(Guid.Empty, id);
        await _repositorio.Received(1).AdicionarAsync(Arg.Any<Produto>(), Arg.Any<CancellationToken>());
        await _repositorio.Received(1).SalvarAsync(Arg.Any<CancellationToken>());
    }
}

public class ExcluirProdutoCommandHandlerTestes
{
    private readonly IProdutoRepositorio _repositorio = Substitute.For<IProdutoRepositorio>();
    private readonly IConsultaMovimentos _movimentos = Substitute.For<IConsultaMovimentos>();

    [Fact]
    public async Task Produto_inexistente_devolve_false()
    {
        _repositorio.ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Produto?)null);
        var handler = new ExcluirProdutoCommandHandler(_repositorio, _movimentos);

        var encontrado = await handler.HandleAsync(Guid.NewGuid(), default);

        Assert.False(encontrado);
    }

    [Fact]
    public async Task Produto_ja_usado_em_movimento_nao_pode_ser_excluido()
    {
        var produto = Produto.Criar("ABC", "Banana", "UN", 1m, 2m);
        _repositorio.ObterPorIdAsync(produto.Id, Arg.Any<CancellationToken>()).Returns(produto);
        _movimentos.ProdutoUtilizadoAsync(produto.Id, Arg.Any<CancellationToken>()).Returns(true);
        var handler = new ExcluirProdutoCommandHandler(_repositorio, _movimentos);

        var erro = await Assert.ThrowsAsync<RegraDeNegocioExcecao>(
            () => handler.HandleAsync(produto.Id, default));

        Assert.Equal("PRODUTO_EM_USO", erro.Codigo);
        _repositorio.DidNotReceive().Remover(Arg.Any<Produto>());
    }

    [Fact]
    public async Task Produto_nunca_usado_e_excluido()
    {
        var produto = Produto.Criar("ABC", "Banana", "UN", 1m, 2m);
        _repositorio.ObterPorIdAsync(produto.Id, Arg.Any<CancellationToken>()).Returns(produto);
        _movimentos.ProdutoUtilizadoAsync(produto.Id, Arg.Any<CancellationToken>()).Returns(false);
        var handler = new ExcluirProdutoCommandHandler(_repositorio, _movimentos);

        var encontrado = await handler.HandleAsync(produto.Id, default);

        Assert.True(encontrado);
        _repositorio.Received(1).Remover(produto);
    }
}
