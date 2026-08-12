using Vendas.Shared.Excecoes;

namespace Vendas.Movimentos.Domain.Entidades;

public sealed class VendaItem
{
    private VendaItem() { }

    private VendaItem(Guid produtoId, decimal quantidade, decimal precoUnitario)
    {
        if (produtoId == Guid.Empty)
        {
            throw new RegraDeNegocioExcecao("ITEM_PRODUTO_OBRIGATORIO", "Informe o produto do item.");
        }

        if (quantidade <= 0)
        {
            throw new RegraDeNegocioExcecao("ITEM_QUANTIDADE_INVALIDA", "A quantidade do item deve ser maior que zero.");
        }

        if (precoUnitario < 0)
        {
            throw new RegraDeNegocioExcecao("ITEM_PRECO_NEGATIVO", "O preço unitário não pode ser negativo.");
        }

        Id = Guid.NewGuid();
        ProdutoId = produtoId;
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
        Subtotal = quantidade * precoUnitario;
    }

    public Guid Id { get; private set; }
    public Guid VendaId { get; private set; }
    public Guid ProdutoId { get; private set; }
    public decimal Quantidade { get; private set; }
    public decimal PrecoUnitario { get; private set; }
    public decimal Subtotal { get; private set; }

    public static VendaItem Criar(Guid produtoId, decimal quantidade, decimal precoUnitario)
        => new(produtoId, quantidade, precoUnitario);
}
