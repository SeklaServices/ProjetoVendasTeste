using Vendas.Shared.Excecoes;

namespace Vendas.Movimentos.Domain.Entidades;

/// <summary>
/// Saída de mercadoria. Não movimenta estoque, e por isso vender um produto que nunca foi comprado
/// é válido — não é bug (docs/07-decisoes.md D-002).
/// </summary>
public sealed class Venda
{
    public const int TamanhoMaximoParceiro = 120;
    public const int TamanhoMaximoObservacao = 500;

    private readonly List<VendaItem> _itens = [];

    private Venda() { }

    private Venda(DateOnly data, string cliente, string? observacao, IEnumerable<VendaItem> itens)
    {
        Id = Guid.NewGuid();
        DataCriacao = DateTime.UtcNow;

        cliente = (cliente ?? string.Empty).Trim();
        observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();

        if (data > DateOnly.FromDateTime(DateTime.Today))
        {
            throw new RegraDeNegocioExcecao("VENDA_DATA_FUTURA", "A data da venda não pode ser futura.");
        }

        if (cliente.Length == 0)
        {
            throw new RegraDeNegocioExcecao("VENDA_CLIENTE_OBRIGATORIO", "Informe o cliente.");
        }

        if (cliente.Length > TamanhoMaximoParceiro)
        {
            throw new RegraDeNegocioExcecao(
                "VENDA_CLIENTE_LONGO", $"O cliente deve ter no máximo {TamanhoMaximoParceiro} caracteres.");
        }

        if (observacao is { Length: > TamanhoMaximoObservacao })
        {
            throw new RegraDeNegocioExcecao(
                "VENDA_OBSERVACAO_LONGA", $"A observação deve ter no máximo {TamanhoMaximoObservacao} caracteres.");
        }

        _itens.AddRange(itens);

        if (_itens.Count == 0)
        {
            throw new RegraDeNegocioExcecao("VENDA_SEM_ITENS", "A venda precisa ter pelo menos um item.");
        }

        Data = data;
        Cliente = cliente;
        Observacao = observacao;
        RecalcularTotal();
    }

    public Guid Id { get; private set; }
    public int Numero { get; private set; }
    public DateOnly Data { get; private set; }
    public string Cliente { get; private set; } = string.Empty;
    public string? Observacao { get; private set; }
    public decimal ValorTotal { get; private set; }
    public DateTime DataCriacao { get; private set; }

    public IReadOnlyList<VendaItem> Itens => _itens;

    public static Venda Criar(DateOnly data, string cliente, string? observacao, IEnumerable<VendaItem> itens)
        => new(data, cliente, observacao, itens);

    private void RecalcularTotal() => ValorTotal = _itens.Sum(item => item.Subtotal);
}
