using Vendas.Movimentos.Domain.Entidades;
using Vendas.Shared.Excecoes;
using Xunit;

namespace Vendas.Movimentos.Tests.Unit;

public class VendaTestes
{
    private static readonly DateOnly Hoje = DateOnly.FromDateTime(DateTime.Today);
    private static readonly Guid ClienteQualquer = Guid.NewGuid();

    private static VendaItem Item(decimal quantidade = 2m, decimal preco = 10m)
        => VendaItem.Criar(Guid.NewGuid(), quantidade, preco);

    [Fact]
    public void Valor_total_e_a_soma_dos_subtotais()
    {
        var venda = Venda.Criar(Hoje, ClienteQualquer, null, [Item(2m, 10m), Item(3m, 5m)]);

        Assert.Equal(35m, venda.ValorTotal);
    }

    [Fact]
    public void Valor_total_considera_todos_os_itens_mesmo_com_muitos()
    {
        var itens = Enumerable.Range(0, 9).Select(_ => Item(1m, 3m)).ToList();

        var venda = Venda.Criar(Hoje, ClienteQualquer, null, itens);

        Assert.Equal(27m, venda.ValorTotal);
        Assert.Equal(9, venda.Itens.Count);
    }

    [Fact]
    public void Venda_sem_itens_e_rejeitada()
    {
        var erro = Assert.Throws<RegraDeNegocioExcecao>(() => Venda.Criar(Hoje, ClienteQualquer, null, []));

        Assert.Equal("VENDA_SEM_ITENS", erro.Codigo);
    }

    [Fact]
    public void Data_futura_e_rejeitada()
    {
        var erro = Assert.Throws<RegraDeNegocioExcecao>(
            () => Venda.Criar(Hoje.AddDays(1), ClienteQualquer, null, [Item()]));

        Assert.Equal("VENDA_DATA_FUTURA", erro.Codigo);
    }

    [Fact]
    public void Cliente_nao_informado_e_rejeitado()
    {
        var erro = Assert.Throws<RegraDeNegocioExcecao>(
            () => Venda.Criar(Hoje, Guid.Empty, null, [Item()]));

        Assert.Equal("VENDA_CLIENTE_OBRIGATORIO", erro.Codigo);
    }

    [Fact]
    public void Venda_guarda_o_id_do_cliente_e_nao_o_nome()
    {
        // O nome vem do cadastro a cada consulta (D-009): renomear o cliente renomeia no histórico.
        var venda = Venda.Criar(Hoje, ClienteQualquer, null, [Item()]);

        Assert.Equal(ClienteQualquer, venda.ClienteId);
    }

    [Fact]
    public void Vender_produto_nunca_comprado_e_valido()
    {
        // Não existe estoque neste sistema (docs/07-decisoes.md D-002). Este teste existe para
        // documentar a decisão: se alguém "corrigir" isso, o teste vermelho explica o porquê.
        var venda = Venda.Criar(Hoje, ClienteQualquer, null, [Item(1000m, 1m)]);

        Assert.Equal(1000m, venda.ValorTotal);
    }
}
