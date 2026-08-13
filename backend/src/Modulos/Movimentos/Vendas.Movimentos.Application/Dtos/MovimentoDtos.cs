namespace Vendas.Movimentos.Application.Dtos;

/// <summary>Linha da listagem. Não carrega os itens — a grid não precisa deles.</summary>
public sealed record CompraResumoDto(
    Guid Id, int Numero, DateOnly Data, string Fornecedor, decimal ValorTotal, int QuantidadeItens);

/// <summary>
/// O nome do cliente vem resolvido do cadastro a cada consulta — a venda guarda só o id (D-009).
/// Se o cliente tiver sido excluído, algo que a regra de negócio impede, o nome cai no fallback.
/// </summary>
public sealed record VendaResumoDto(
    Guid Id,
    int Numero,
    DateOnly Data,
    Guid ClienteId,
    int ClienteCodigo,
    string ClienteNome,
    decimal ValorTotal,
    int QuantidadeItens);

/// <summary>Item já com os dados do produto resolvidos, para a tela não precisar de outra chamada.</summary>
public sealed record ItemDto(
    Guid Id,
    Guid ProdutoId,
    string ProdutoCodigo,
    string ProdutoNome,
    string UnidadeMedida,
    decimal Quantidade,
    decimal PrecoUnitario,
    decimal Subtotal);

public sealed record CompraDetalheDto(
    Guid Id,
    int Numero,
    DateOnly Data,
    string Fornecedor,
    string? Observacao,
    decimal ValorTotal,
    DateTime DataCriacao,
    IReadOnlyList<ItemDto> Itens);

public sealed record VendaDetalheDto(
    Guid Id,
    int Numero,
    DateOnly Data,
    Guid ClienteId,
    int ClienteCodigo,
    string ClienteNome,
    string? Observacao,
    decimal ValorTotal,
    DateTime DataCriacao,
    IReadOnlyList<ItemDto> Itens);

public sealed record ResumoDto(
    decimal TotalComprado,
    decimal TotalVendido,
    decimal Diferenca,
    int QuantidadeCompras,
    int QuantidadeVendas,
    int ProdutosCadastrados,
    int ProdutosAtivos,
    IReadOnlyList<CompraResumoDto> UltimasCompras,
    IReadOnlyList<VendaResumoDto> UltimasVendas);
