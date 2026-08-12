namespace Vendas.Cadastros.Application.Dtos;

/// <summary>O que a API devolve. Separado da entidade de propósito: mudar a entidade não deve
/// quebrar o contrato do frontend sem que alguém perceba.</summary>
public sealed record ProdutoDto(
    Guid Id,
    string Codigo,
    string Nome,
    string UnidadeMedida,
    decimal PrecoCusto,
    decimal PrecoVenda,
    bool Ativo,
    DateTime DataCadastro);
