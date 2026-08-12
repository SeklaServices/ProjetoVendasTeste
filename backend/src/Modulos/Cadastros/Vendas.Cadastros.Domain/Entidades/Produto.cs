using Vendas.Shared.Excecoes;

namespace Vendas.Cadastros.Domain.Entidades;

/// <summary>
/// Produto do catálogo. Não tem saldo nem estoque — por decisão de escopo (docs/07-decisoes.md D-002).
///
/// As regras vivem aqui, e não no endpoint nem no repositório: é a única forma de garantir que um
/// produto inválido não exista, independentemente de quem tenha chamado.
/// </summary>
public sealed class Produto
{
    public const int TamanhoMaximoCodigo = 20;
    public const int TamanhoMaximoNome = 120;
    public const int TamanhoMaximoUnidade = 10;

    // Construtor privado para o EF Core materializar a entidade sem passar pelas validações.
    private Produto() { }

    private Produto(string codigo, string nome, string unidadeMedida, decimal precoCusto, decimal precoVenda)
    {
        Id = Guid.NewGuid();
        DataCadastro = DateTime.UtcNow;
        Ativo = true;
        AplicarDados(codigo, nome, unidadeMedida, precoCusto, precoVenda);
    }

    public Guid Id { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Nome { get; private set; } = string.Empty;
    public string UnidadeMedida { get; private set; } = string.Empty;
    public decimal PrecoCusto { get; private set; }
    public decimal PrecoVenda { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime DataCadastro { get; private set; }

    public static Produto Criar(string codigo, string nome, string unidadeMedida, decimal precoCusto, decimal precoVenda)
        => new(codigo, nome, unidadeMedida, precoCusto, precoVenda);

    public void Atualizar(string codigo, string nome, string unidadeMedida, decimal precoCusto, decimal precoVenda, bool ativo)
    {
        AplicarDados(codigo, nome, unidadeMedida, precoCusto, precoVenda);
        Ativo = ativo;
    }

    public void Inativar() => Ativo = false;

    public void Reativar() => Ativo = true;

    private void AplicarDados(string codigo, string nome, string unidadeMedida, decimal precoCusto, decimal precoVenda)
    {
        codigo = (codigo ?? string.Empty).Trim();
        nome = (nome ?? string.Empty).Trim();
        unidadeMedida = (unidadeMedida ?? string.Empty).Trim().ToUpperInvariant();

        if (codigo.Length == 0)
        {
            throw new RegraDeNegocioExcecao("PRODUTO_CODIGO_OBRIGATORIO", "Informe o código do produto.");
        }

        if (codigo.Length > TamanhoMaximoCodigo)
        {
            throw new RegraDeNegocioExcecao(
                "PRODUTO_CODIGO_LONGO", $"O código deve ter no máximo {TamanhoMaximoCodigo} caracteres.");
        }

        if (nome.Length == 0)
        {
            throw new RegraDeNegocioExcecao("PRODUTO_NOME_OBRIGATORIO", "Informe o nome do produto.");
        }

        if (nome.Length > TamanhoMaximoNome)
        {
            throw new RegraDeNegocioExcecao(
                "PRODUTO_NOME_LONGO", $"O nome deve ter no máximo {TamanhoMaximoNome} caracteres.");
        }

        if (unidadeMedida.Length == 0)
        {
            throw new RegraDeNegocioExcecao(
                "PRODUTO_UNIDADE_OBRIGATORIA", "Informe a unidade de medida (ex.: UN, KG, CX).");
        }

        if (unidadeMedida.Length > TamanhoMaximoUnidade)
        {
            throw new RegraDeNegocioExcecao(
                "PRODUTO_UNIDADE_LONGA", $"A unidade deve ter no máximo {TamanhoMaximoUnidade} caracteres.");
        }

        if (precoCusto < 0)
        {
            throw new RegraDeNegocioExcecao("PRODUTO_CUSTO_NEGATIVO", "O preço de custo não pode ser negativo.");
        }

        if (precoVenda < 0)
        {
            throw new RegraDeNegocioExcecao("PRODUTO_VENDA_NEGATIVO", "O preço de venda não pode ser negativo.");
        }

        Codigo = codigo;
        Nome = nome;
        UnidadeMedida = unidadeMedida;
        PrecoCusto = precoCusto;
        PrecoVenda = precoVenda;
    }
}
