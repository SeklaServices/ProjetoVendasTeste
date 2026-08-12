using NSubstitute;
using Vendas.Movimentos.Application.Commands;
using Vendas.Movimentos.Domain.Entidades;
using Vendas.Movimentos.Domain.Interfaces;
using Vendas.Shared.Contratos;
using Vendas.Shared.Excecoes;
using Xunit;

namespace Vendas.Movimentos.Tests.Unit;

public class CriarVendaCommandHandlerTestes
{
    private static readonly DateOnly Hoje = DateOnly.FromDateTime(DateTime.Today);

    private readonly IVendaRepositorio _repositorio = Substitute.For<IVendaRepositorio>();
    private readonly IConsultaProdutos _produtos = Substitute.For<IConsultaProdutos>();
    private readonly IConsultaClientes _clientes = Substitute.For<IConsultaClientes>();

    private readonly Guid _produtoId = Guid.NewGuid();
    private readonly Guid _clienteId = Guid.NewGuid();

    public CriarVendaCommandHandlerTestes()
    {
        _produtos.ObterPorIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([new ProdutoResumo(_produtoId, "ABC", "Banana", "KG", true)]);
    }

    private CriarVendaCommandHandler CriarHandler() => new(_repositorio, _produtos, _clientes);

    private void ConfigurarCliente(bool ativo) =>
        _clientes.ObterPorIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([new ClienteResumo(_clienteId, 7, "Mercado Central", ativo)]);

    private CriarVendaCommand Comando() =>
        new(Hoje, _clienteId, null, [new ItemCommand(_produtoId, 2m, 10m)]);

    [Fact]
    public async Task Venda_com_cliente_ativo_e_persistida()
    {
        ConfigurarCliente(ativo: true);

        await CriarHandler().HandleAsync(Comando(), default);

        await _repositorio.Received(1).AdicionarAsync(Arg.Any<Venda>(), Arg.Any<CancellationToken>());
        await _repositorio.Received(1).SalvarAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cliente_inexistente_e_rejeitado()
    {
        _clientes.ObterPorIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var erro = await Assert.ThrowsAsync<RegraDeNegocioExcecao>(
            () => CriarHandler().HandleAsync(Comando(), default));

        Assert.Equal("VENDA_CLIENTE_INEXISTENTE", erro.Codigo);
        await _repositorio.DidNotReceive().AdicionarAsync(Arg.Any<Venda>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cliente_inativo_e_rejeitado()
    {
        ConfigurarCliente(ativo: false);

        var erro = await Assert.ThrowsAsync<RegraDeNegocioExcecao>(
            () => CriarHandler().HandleAsync(Comando(), default));

        Assert.Equal("VENDA_CLIENTE_INATIVO", erro.Codigo);
        await _repositorio.DidNotReceive().SalvarAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cliente_e_verificado_antes_dos_produtos()
    {
        // Cliente inválido não deve nem chegar a consultar o catálogo de produtos: a venda já
        // está condenada, e a mensagem que interessa ao usuário é a do cliente.
        _clientes.ObterPorIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        await Assert.ThrowsAsync<RegraDeNegocioExcecao>(
            () => CriarHandler().HandleAsync(Comando(), default));

        await _produtos.DidNotReceive().ObterPorIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
    }
}
