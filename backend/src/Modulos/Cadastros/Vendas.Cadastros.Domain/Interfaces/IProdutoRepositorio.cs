using Vendas.Cadastros.Domain.Entidades;

namespace Vendas.Cadastros.Domain.Interfaces;

/// <summary>
/// Persistência de produtos. A interface vive no Domain, a implementação em Infrastructure — é
/// isso que permite testar os handlers sem banco nenhum.
/// </summary>
public interface IProdutoRepositorio
{
    Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken ct);

    /// <param name="idIgnorado">Na edição, o próprio produto não conta como duplicidade.</param>
    Task<bool> ExisteCodigoAsync(string codigo, Guid? idIgnorado, CancellationToken ct);

    Task<IReadOnlyList<Produto>> ListarAsync(string? busca, bool apenasAtivos, CancellationToken ct);

    Task AdicionarAsync(Produto produto, CancellationToken ct);

    void Remover(Produto produto);

    Task SalvarAsync(CancellationToken ct);
}
