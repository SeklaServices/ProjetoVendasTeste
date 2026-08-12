using Vendas.Cadastros.Domain.Entidades;
using Vendas.Shared.Excecoes;
using Xunit;

namespace Vendas.Cadastros.Tests.Unit;

/// <summary>
/// Regras da entidade. Nenhum mock, nenhum banco — a entidade não depende de nada.
/// É esta suíte que fica vermelha quando alguém afrouxa uma validação.
/// </summary>
public class ProdutoTestes
{
    [Fact]
    public void Criar_produto_valido_nasce_ativo()
    {
        var produto = Produto.Criar("ABC", "Banana Prata", "kg", 4.50m, 7.90m);

        Assert.True(produto.Ativo);
        Assert.Equal("ABC", produto.Codigo);
        Assert.Equal("Banana Prata", produto.Nome);
        Assert.NotEqual(Guid.Empty, produto.Id);
    }

    [Fact]
    public void Unidade_de_medida_e_normalizada_para_maiuscula()
    {
        var produto = Produto.Criar("ABC", "Banana", "kg", 1m, 2m);

        Assert.Equal("KG", produto.UnidadeMedida);
    }

    [Fact]
    public void Codigo_e_nome_sao_normalizados_sem_espacos_nas_pontas()
    {
        var produto = Produto.Criar("  ABC  ", "  Banana  ", "UN", 1m, 2m);

        Assert.Equal("ABC", produto.Codigo);
        Assert.Equal("Banana", produto.Nome);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Codigo_vazio_e_rejeitado(string codigo)
    {
        var erro = Assert.Throws<RegraDeNegocioExcecao>(
            () => Produto.Criar(codigo, "Banana", "UN", 1m, 2m));

        Assert.Equal("PRODUTO_CODIGO_OBRIGATORIO", erro.Codigo);
    }

    [Fact]
    public void Nome_vazio_e_rejeitado()
    {
        var erro = Assert.Throws<RegraDeNegocioExcecao>(() => Produto.Criar("ABC", "", "UN", 1m, 2m));

        Assert.Equal("PRODUTO_NOME_OBRIGATORIO", erro.Codigo);
    }

    [Fact]
    public void Unidade_vazia_e_rejeitada()
    {
        var erro = Assert.Throws<RegraDeNegocioExcecao>(() => Produto.Criar("ABC", "Banana", "", 1m, 2m));

        Assert.Equal("PRODUTO_UNIDADE_OBRIGATORIA", erro.Codigo);
    }

    [Fact]
    public void Preco_de_custo_negativo_e_rejeitado()
    {
        var erro = Assert.Throws<RegraDeNegocioExcecao>(
            () => Produto.Criar("ABC", "Banana", "UN", -1m, 2m));

        Assert.Equal("PRODUTO_CUSTO_NEGATIVO", erro.Codigo);
    }

    [Fact]
    public void Preco_de_venda_negativo_e_rejeitado()
    {
        var erro = Assert.Throws<RegraDeNegocioExcecao>(
            () => Produto.Criar("ABC", "Banana", "UN", 1m, -2m));

        Assert.Equal("PRODUTO_VENDA_NEGATIVO", erro.Codigo);
    }

    [Fact]
    public void Codigo_acima_do_tamanho_maximo_e_rejeitado()
    {
        var codigoLongo = new string('A', Produto.TamanhoMaximoCodigo + 1);

        var erro = Assert.Throws<RegraDeNegocioExcecao>(
            () => Produto.Criar(codigoLongo, "Banana", "UN", 1m, 2m));

        Assert.Equal("PRODUTO_CODIGO_LONGO", erro.Codigo);
    }

    [Fact]
    public void Preco_de_venda_menor_que_o_custo_e_permitido()
    {
        // Prejuízo é decisão comercial, não erro de sistema. A tela avisa; o domínio não bloqueia.
        var produto = Produto.Criar("ABC", "Banana", "UN", 10m, 5m);

        Assert.Equal(5m, produto.PrecoVenda);
    }

    [Fact]
    public void Inativar_e_reativar_alternam_a_situacao()
    {
        var produto = Produto.Criar("ABC", "Banana", "UN", 1m, 2m);

        produto.Inativar();
        Assert.False(produto.Ativo);

        produto.Reativar();
        Assert.True(produto.Ativo);
    }
}
