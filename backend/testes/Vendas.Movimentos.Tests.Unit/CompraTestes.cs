using Vendas.Movimentos.Domain.Entidades;
using Vendas.Shared.Excecoes;
using Xunit;

namespace Vendas.Movimentos.Tests.Unit;

public class CompraTestes
{
    private static readonly DateOnly Hoje = DateOnly.FromDateTime(DateTime.Today);

    private static CompraItem Item(decimal quantidade = 2m, decimal preco = 10m)
        => CompraItem.Criar(Guid.NewGuid(), quantidade, preco);

    [Fact]
    public void Valor_total_e_a_soma_dos_subtotais()
    {
        var compra = Compra.Criar(Hoje, "Fornecedor X", null,
            [Item(2m, 10m), Item(3m, 5m), Item(1m, 2.5m)]);

        Assert.Equal(37.5m, compra.ValorTotal);
    }

    [Fact]
    public void Valor_total_considera_todos_os_itens_mesmo_com_muitos()
    {
        // Guarda contra o erro clássico de laço que para um item antes do fim.
        var itens = Enumerable.Range(0, 7).Select(_ => Item(1m, 10m)).ToList();

        var compra = Compra.Criar(Hoje, "Fornecedor X", null, itens);

        Assert.Equal(70m, compra.ValorTotal);
        Assert.Equal(7, compra.Itens.Count);
    }

    [Fact]
    public void Compra_sem_itens_e_rejeitada()
    {
        var erro = Assert.Throws<RegraDeNegocioExcecao>(
            () => Compra.Criar(Hoje, "Fornecedor X", null, []));

        Assert.Equal("COMPRA_SEM_ITENS", erro.Codigo);
    }

    [Fact]
    public void Data_futura_e_rejeitada()
    {
        var amanha = Hoje.AddDays(1);

        var erro = Assert.Throws<RegraDeNegocioExcecao>(
            () => Compra.Criar(amanha, "Fornecedor X", null, [Item()]));

        Assert.Equal("COMPRA_DATA_FUTURA", erro.Codigo);
    }

    [Fact]
    public void Data_de_hoje_e_aceita()
    {
        var compra = Compra.Criar(Hoje, "Fornecedor X", null, [Item()]);

        Assert.Equal(Hoje, compra.Data);
    }

    [Fact]
    public void Fornecedor_vazio_e_rejeitado()
    {
        var erro = Assert.Throws<RegraDeNegocioExcecao>(
            () => Compra.Criar(Hoje, "   ", null, [Item()]));

        Assert.Equal("COMPRA_FORNECEDOR_OBRIGATORIO", erro.Codigo);
    }

    [Fact]
    public void Observacao_em_branco_vira_nula()
    {
        var compra = Compra.Criar(Hoje, "Fornecedor X", "   ", [Item()]);

        Assert.Null(compra.Observacao);
    }

    [Fact]
    public void Observacao_acima_do_tamanho_maximo_e_rejeitada()
    {
        var longa = new string('x', Compra.TamanhoMaximoObservacao + 1);

        var erro = Assert.Throws<RegraDeNegocioExcecao>(
            () => Compra.Criar(Hoje, "Fornecedor X", longa, [Item()]));

        Assert.Equal("COMPRA_OBSERVACAO_LONGA", erro.Codigo);
    }
}

public class CompraItemTestes
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Quantidade_menor_ou_igual_a_zero_e_rejeitada(decimal quantidade)
    {
        var erro = Assert.Throws<RegraDeNegocioExcecao>(
            () => CompraItem.Criar(Guid.NewGuid(), quantidade, 10m));

        Assert.Equal("ITEM_QUANTIDADE_INVALIDA", erro.Codigo);
    }

    [Fact]
    public void Preco_negativo_e_rejeitado()
    {
        var erro = Assert.Throws<RegraDeNegocioExcecao>(
            () => CompraItem.Criar(Guid.NewGuid(), 1m, -0.01m));

        Assert.Equal("ITEM_PRECO_NEGATIVO", erro.Codigo);
    }

    [Fact]
    public void Preco_zero_e_permitido()
    {
        // Bonificação, brinde, amostra. Não é erro.
        var item = CompraItem.Criar(Guid.NewGuid(), 5m, 0m);

        Assert.Equal(0m, item.Subtotal);
    }

    [Fact]
    public void Produto_nao_informado_e_rejeitado()
    {
        var erro = Assert.Throws<RegraDeNegocioExcecao>(
            () => CompraItem.Criar(Guid.Empty, 1m, 10m));

        Assert.Equal("ITEM_PRODUTO_OBRIGATORIO", erro.Codigo);
    }

    [Fact]
    public void Subtotal_e_quantidade_vezes_preco()
    {
        var item = CompraItem.Criar(Guid.NewGuid(), 2.5m, 4m);

        Assert.Equal(10m, item.Subtotal);
    }
}
