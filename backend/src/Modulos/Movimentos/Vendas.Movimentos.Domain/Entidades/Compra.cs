using Vendas.Shared.Excecoes;

namespace Vendas.Movimentos.Domain.Entidades;

/// <summary>
/// Entrada de mercadoria. Não movimenta estoque — estoque não existe neste sistema
/// (docs/07-decisoes.md D-002).
///
/// Compra e <see cref="Venda"/> são hoje estruturalmente iguais e estão duplicadas de propósito:
/// generalizar as duas é uma decisão que a equipe deve tomar em um PR próprio, com a discussão
/// registrada no review.
/// </summary>
public sealed class Compra
{
    public const int TamanhoMaximoParceiro = 120;
    public const int TamanhoMaximoObservacao = 500;

    private readonly List<CompraItem> _itens = [];

    private Compra() { }

    private Compra(DateOnly data, string fornecedor, string? observacao, IEnumerable<CompraItem> itens)
    {
        Id = Guid.NewGuid();
        DataCriacao = DateTime.UtcNow;

        fornecedor = (fornecedor ?? string.Empty).Trim();
        observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();

        if (data > DateOnly.FromDateTime(DateTime.Today))
        {
            throw new RegraDeNegocioExcecao("COMPRA_DATA_FUTURA", "A data da compra não pode ser futura.");
        }

        if (fornecedor.Length == 0)
        {
            throw new RegraDeNegocioExcecao("COMPRA_FORNECEDOR_OBRIGATORIO", "Informe o fornecedor.");
        }

        if (fornecedor.Length > TamanhoMaximoParceiro)
        {
            throw new RegraDeNegocioExcecao(
                "COMPRA_FORNECEDOR_LONGO", $"O fornecedor deve ter no máximo {TamanhoMaximoParceiro} caracteres.");
        }

        if (observacao is { Length: > TamanhoMaximoObservacao })
        {
            throw new RegraDeNegocioExcecao(
                "COMPRA_OBSERVACAO_LONGA", $"A observação deve ter no máximo {TamanhoMaximoObservacao} caracteres.");
        }

        _itens.AddRange(itens);

        if (_itens.Count == 0)
        {
            throw new RegraDeNegocioExcecao("COMPRA_SEM_ITENS", "A compra precisa ter pelo menos um item.");
        }

        Data = data;
        Fornecedor = fornecedor;
        Observacao = observacao;
        RecalcularTotal();
    }

    public Guid Id { get; private set; }

    /// <summary>Sequencial, gerado pelo banco (SEQUENCE). Só tem valor depois de salvo.</summary>
    public int Numero { get; private set; }

    public DateOnly Data { get; private set; }
    public string Fornecedor { get; private set; } = string.Empty;
    public string? Observacao { get; private set; }
    public decimal ValorTotal { get; private set; }
    public DateTime DataCriacao { get; private set; }

    public IReadOnlyList<CompraItem> Itens => _itens;

    public static Compra Criar(DateOnly data, string fornecedor, string? observacao, IEnumerable<CompraItem> itens)
        => new(data, fornecedor, observacao, itens);

    /// <summary>O total é sempre derivado dos itens. Nunca aceita valor vindo de fora.</summary>
    private void RecalcularTotal() => ValorTotal = _itens.Sum(item => item.Subtotal);
}
