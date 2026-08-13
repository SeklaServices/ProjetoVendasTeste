using Vendas.Cadastros.Application.Dtos;
using Vendas.Cadastros.Domain.Entidades;
using Vendas.Cadastros.Domain.Interfaces;

namespace Vendas.Cadastros.Application.Queries;

public sealed record ListarClientesQuery(string? Busca, bool ApenasAtivos);

public sealed class ListarClientesQueryHandler(IClienteRepositorio repositorio)
{
    public async Task<IReadOnlyList<ClienteDto>> HandleAsync(ListarClientesQuery consulta, CancellationToken ct)
    {
        var clientes = await repositorio.ListarAsync(consulta.Busca, consulta.ApenasAtivos, ct);
        return clientes.Select(Mapear).ToList();
    }

    internal static ClienteDto Mapear(Cliente c) => new(
        c.Id, c.Codigo, c.Nome, c.Documento, c.Telefone, c.Email, c.Ativo, c.DataCadastro);
}

public sealed class ObterClienteQueryHandler(IClienteRepositorio repositorio)
{
    public async Task<ClienteDto?> HandleAsync(Guid id, CancellationToken ct)
    {
        var cliente = await repositorio.ObterPorIdAsync(id, ct);
        return cliente is null ? null : ListarClientesQueryHandler.Mapear(cliente);
    }
}
