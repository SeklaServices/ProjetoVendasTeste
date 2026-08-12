namespace Vendas.Cadastros.Application.Dtos;

public sealed record ClienteDto(
    Guid Id,
    int Codigo,
    string Nome,
    string? Documento,
    string? Telefone,
    string? Email,
    bool Ativo,
    DateTime DataCadastro);
