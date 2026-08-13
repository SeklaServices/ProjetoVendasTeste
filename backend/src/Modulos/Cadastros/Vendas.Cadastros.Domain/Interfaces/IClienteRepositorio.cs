using Vendas.Cadastros.Domain.Entidades;

namespace Vendas.Cadastros.Domain.Interfaces;

public interface IClienteRepositorio
{
    Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken ct);

    /// <param name="idIgnorado">Na edição, o próprio cliente não conta como duplicidade.</param>
    Task<bool> ExisteDocumentoAsync(string documento, Guid? idIgnorado, CancellationToken ct);

    Task<IReadOnlyList<Cliente>> ListarAsync(string? busca, bool apenasAtivos, CancellationToken ct);

    Task AdicionarAsync(Cliente cliente, CancellationToken ct);

    void Remover(Cliente cliente);

    Task SalvarAsync(CancellationToken ct);
}
