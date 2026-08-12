using Vendas.Cadastros.Domain.Entidades;
using Vendas.Cadastros.Domain.Interfaces;
using Vendas.Shared.Contratos;
using Vendas.Shared.Excecoes;

namespace Vendas.Cadastros.Application.Commands;

public sealed record CriarClienteCommand(
    string Nome, string? Documento, string? Telefone, string? Email);

public sealed class CriarClienteCommandHandler(IClienteRepositorio repositorio)
{
    public async Task<Guid> HandleAsync(CriarClienteCommand comando, CancellationToken ct)
    {
        await GarantirDocumentoDisponivelAsync(repositorio, comando.Documento, idIgnorado: null, ct);

        var cliente = Cliente.Criar(comando.Nome, comando.Documento, comando.Telefone, comando.Email);

        await repositorio.AdicionarAsync(cliente, ct);
        await repositorio.SalvarAsync(ct);

        return cliente.Id;
    }

    /// <summary>
    /// Unicidade é regra de caso de uso, não de entidade: a entidade não enxerga os outros clientes.
    /// Compartilhada entre criar e atualizar para as duas se comportarem igual.
    /// </summary>
    internal static async Task GarantirDocumentoDisponivelAsync(
        IClienteRepositorio repositorio, string? documento, Guid? idIgnorado, CancellationToken ct)
    {
        var normalizado = Cliente.NormalizarDocumento(documento);
        if (normalizado is null)
        {
            return;
        }

        if (await repositorio.ExisteDocumentoAsync(normalizado, idIgnorado, ct))
        {
            throw new RegraDeNegocioExcecao(
                "CLIENTE_DOCUMENTO_DUPLICADO", $"Já existe um cliente com o documento '{documento}'.");
        }
    }
}

public sealed record AtualizarClienteCommand(
    Guid Id, string Nome, string? Documento, string? Telefone, string? Email, bool Ativo);

public sealed class AtualizarClienteCommandHandler(IClienteRepositorio repositorio)
{
    /// <returns><c>false</c> quando o cliente não existe — o endpoint traduz em 404.</returns>
    public async Task<bool> HandleAsync(AtualizarClienteCommand comando, CancellationToken ct)
    {
        var cliente = await repositorio.ObterPorIdAsync(comando.Id, ct);
        if (cliente is null)
        {
            return false;
        }

        await CriarClienteCommandHandler.GarantirDocumentoDisponivelAsync(
            repositorio, comando.Documento, idIgnorado: comando.Id, ct);

        cliente.Atualizar(comando.Nome, comando.Documento, comando.Telefone, comando.Email, comando.Ativo);

        await repositorio.SalvarAsync(ct);
        return true;
    }
}

public sealed class ExcluirClienteCommandHandler(
    IClienteRepositorio repositorio,
    IConsultaMovimentos consultaMovimentos)
{
    public async Task<bool> HandleAsync(Guid id, CancellationToken ct)
    {
        var cliente = await repositorio.ObterPorIdAsync(id, ct);
        if (cliente is null)
        {
            return false;
        }

        // Não existe FK entre módulos (D-007) — é esta consulta que garante a integridade, e ela
        // devolve uma mensagem útil em vez de uma violação de constraint.
        if (await consultaMovimentos.ClienteUtilizadoAsync(id, ct))
        {
            throw new RegraDeNegocioExcecao(
                "CLIENTE_EM_USO",
                "Este cliente já tem vendas registradas e não pode ser excluído. Inative-o.");
        }

        repositorio.Remover(cliente);
        await repositorio.SalvarAsync(ct);
        return true;
    }
}
