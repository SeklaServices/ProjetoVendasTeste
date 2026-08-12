using NSubstitute;
using Vendas.Cadastros.Application.Commands;
using Vendas.Cadastros.Domain.Entidades;
using Vendas.Cadastros.Domain.Interfaces;
using Vendas.Shared.Contratos;
using Vendas.Shared.Excecoes;
using Xunit;

namespace Vendas.Cadastros.Tests.Unit;

public class CriarClienteCommandHandlerTestes
{
    private readonly IClienteRepositorio _repositorio = Substitute.For<IClienteRepositorio>();

    [Fact]
    public async Task Documento_duplicado_e_rejeitado()
    {
        _repositorio.ExisteDocumentoAsync("12345678909", null, Arg.Any<CancellationToken>()).Returns(true);
        var handler = new CriarClienteCommandHandler(_repositorio);

        var erro = await Assert.ThrowsAsync<RegraDeNegocioExcecao>(() =>
            handler.HandleAsync(new CriarClienteCommand("Jose", "123.456.789-09", null, null), default));

        Assert.Equal("CLIENTE_DOCUMENTO_DUPLICADO", erro.Codigo);
        await _repositorio.DidNotReceive().AdicionarAsync(Arg.Any<Cliente>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cliente_sem_documento_nao_verifica_duplicidade()
    {
        // Documento é opcional. Vários clientes sem documento convivem — por isso o índice único
        // do banco é filtrado por "IS NOT NULL".
        var handler = new CriarClienteCommandHandler(_repositorio);

        await handler.HandleAsync(new CriarClienteCommand("Mercado", null, null, null), default);

        await _repositorio.DidNotReceive().ExisteDocumentoAsync(
            Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
        await _repositorio.Received(1).AdicionarAsync(Arg.Any<Cliente>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cliente_valido_e_persistido()
    {
        _repositorio.ExisteDocumentoAsync(Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(false);
        var handler = new CriarClienteCommandHandler(_repositorio);

        var id = await handler.HandleAsync(
            new CriarClienteCommand("Mercado", "12345678000190", "1199999", "a@b.com"), default);

        Assert.NotEqual(Guid.Empty, id);
        await _repositorio.Received(1).SalvarAsync(Arg.Any<CancellationToken>());
    }
}

public class AtualizarClienteCommandHandlerTestes
{
    private readonly IClienteRepositorio _repositorio = Substitute.For<IClienteRepositorio>();

    [Fact]
    public async Task Cliente_inexistente_devolve_false()
    {
        _repositorio.ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Cliente?)null);
        var handler = new AtualizarClienteCommandHandler(_repositorio);

        var encontrado = await handler.HandleAsync(
            new AtualizarClienteCommand(Guid.NewGuid(), "Mercado", null, null, null, true), default);

        Assert.False(encontrado);
    }

    [Fact]
    public async Task Proprio_documento_nao_conta_como_duplicidade()
    {
        var cliente = Cliente.Criar("Mercado", "12345678909", null, null);
        _repositorio.ObterPorIdAsync(cliente.Id, Arg.Any<CancellationToken>()).Returns(cliente);
        _repositorio.ExisteDocumentoAsync("12345678909", cliente.Id, Arg.Any<CancellationToken>())
            .Returns(false);
        var handler = new AtualizarClienteCommandHandler(_repositorio);

        var encontrado = await handler.HandleAsync(
            new AtualizarClienteCommand(cliente.Id, "Mercado Central", "12345678909", null, null, true),
            default);

        Assert.True(encontrado);
        Assert.Equal("Mercado Central", cliente.Nome);
    }
}

public class ExcluirClienteCommandHandlerTestes
{
    private readonly IClienteRepositorio _repositorio = Substitute.For<IClienteRepositorio>();
    private readonly IConsultaMovimentos _movimentos = Substitute.For<IConsultaMovimentos>();

    [Fact]
    public async Task Cliente_com_vendas_nao_pode_ser_excluido()
    {
        var cliente = Cliente.Criar("Mercado", null, null, null);
        _repositorio.ObterPorIdAsync(cliente.Id, Arg.Any<CancellationToken>()).Returns(cliente);
        _movimentos.ClienteUtilizadoAsync(cliente.Id, Arg.Any<CancellationToken>()).Returns(true);
        var handler = new ExcluirClienteCommandHandler(_repositorio, _movimentos);

        var erro = await Assert.ThrowsAsync<RegraDeNegocioExcecao>(
            () => handler.HandleAsync(cliente.Id, default));

        Assert.Equal("CLIENTE_EM_USO", erro.Codigo);
        _repositorio.DidNotReceive().Remover(Arg.Any<Cliente>());
    }

    [Fact]
    public async Task Cliente_sem_vendas_e_excluido()
    {
        var cliente = Cliente.Criar("Mercado", null, null, null);
        _repositorio.ObterPorIdAsync(cliente.Id, Arg.Any<CancellationToken>()).Returns(cliente);
        _movimentos.ClienteUtilizadoAsync(cliente.Id, Arg.Any<CancellationToken>()).Returns(false);
        var handler = new ExcluirClienteCommandHandler(_repositorio, _movimentos);

        var encontrado = await handler.HandleAsync(cliente.Id, default);

        Assert.True(encontrado);
        _repositorio.Received(1).Remover(cliente);
    }

    [Fact]
    public async Task Cliente_inexistente_devolve_false()
    {
        _repositorio.ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Cliente?)null);
        var handler = new ExcluirClienteCommandHandler(_repositorio, _movimentos);

        Assert.False(await handler.HandleAsync(Guid.NewGuid(), default));
    }
}
