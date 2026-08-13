using Vendas.Cadastros.Domain.Entidades;
using Vendas.Shared.Excecoes;
using Xunit;

namespace Vendas.Cadastros.Tests.Unit;

public class ClienteTestes
{
    [Fact]
    public void Cliente_valido_nasce_ativo()
    {
        var cliente = Cliente.Criar("Mercado Central", null, null, null);

        Assert.True(cliente.Ativo);
        Assert.Equal("Mercado Central", cliente.Nome);
        Assert.NotEqual(Guid.Empty, cliente.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Nome_vazio_e_rejeitado(string nome)
    {
        var erro = Assert.Throws<RegraDeNegocioExcecao>(() => Cliente.Criar(nome, null, null, null));

        Assert.Equal("CLIENTE_NOME_OBRIGATORIO", erro.Codigo);
    }

    [Fact]
    public void Nome_e_normalizado_sem_espacos_nas_pontas()
    {
        var cliente = Cliente.Criar("  Mercado Central  ", null, null, null);

        Assert.Equal("Mercado Central", cliente.Nome);
    }

    [Fact]
    public void Nome_acima_do_tamanho_maximo_e_rejeitado()
    {
        var longo = new string('x', Cliente.TamanhoMaximoNome + 1);

        var erro = Assert.Throws<RegraDeNegocioExcecao>(() => Cliente.Criar(longo, null, null, null));

        Assert.Equal("CLIENTE_NOME_LONGO", erro.Codigo);
    }

    [Fact]
    public void Documento_e_guardado_so_com_digitos()
    {
        // "12.345.678/0001-90" e "12345678000190" são o mesmo documento. Guardar formatado faria
        // os dois passarem pela verificação de duplicidade.
        var cliente = Cliente.Criar("Mercado", "12.345.678/0001-90", null, null);

        Assert.Equal("12345678000190", cliente.Documento);
    }

    [Fact]
    public void Cpf_com_onze_digitos_e_aceito()
    {
        var cliente = Cliente.Criar("Jose", "123.456.789-09", null, null);

        Assert.Equal("12345678909", cliente.Documento);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("123456789012")]
    public void Documento_com_quantidade_de_digitos_invalida_e_rejeitado(string documento)
    {
        var erro = Assert.Throws<RegraDeNegocioExcecao>(
            () => Cliente.Criar("Mercado", documento, null, null));

        Assert.Equal("CLIENTE_DOCUMENTO_INVALIDO", erro.Codigo);
    }

    [Fact]
    public void Documento_invalido_no_digito_verificador_e_aceito()
    {
        // Limitação conhecida e deliberada (D-009): só o tamanho é verificado. Este teste existe
        // para documentar a decisão — se alguém implementar a validação de DV, ele fica vermelho
        // e a conversa acontece no PR, e não em produção.
        var cliente = Cliente.Criar("Mercado", "11111111111", null, null);

        Assert.Equal("11111111111", cliente.Documento);
    }

    [Fact]
    public void Documento_em_branco_vira_nulo()
    {
        var cliente = Cliente.Criar("Mercado", "   ", null, null);

        Assert.Null(cliente.Documento);
    }

    [Theory]
    [InlineData("semarroba.com")]
    [InlineData("@semlocal.com")]
    [InlineData("sem@dominio")]
    [InlineData("ponto@final.")]
    [InlineData("com espaco@dominio.com")]
    public void Email_invalido_e_rejeitado(string email)
    {
        var erro = Assert.Throws<RegraDeNegocioExcecao>(
            () => Cliente.Criar("Mercado", null, null, email));

        Assert.Equal("CLIENTE_EMAIL_INVALIDO", erro.Codigo);
    }

    [Fact]
    public void Email_valido_e_aceito()
    {
        var cliente = Cliente.Criar("Mercado", null, null, "contato@mercado.com.br");

        Assert.Equal("contato@mercado.com.br", cliente.Email);
    }

    [Fact]
    public void Telefone_acima_do_tamanho_maximo_e_rejeitado()
    {
        var longo = new string('9', Cliente.TamanhoMaximoTelefone + 1);

        var erro = Assert.Throws<RegraDeNegocioExcecao>(
            () => Cliente.Criar("Mercado", null, longo, null));

        Assert.Equal("CLIENTE_TELEFONE_LONGO", erro.Codigo);
    }

    [Fact]
    public void Atualizar_troca_os_dados_e_a_situacao()
    {
        var cliente = Cliente.Criar("Mercado", null, null, null);

        cliente.Atualizar("Mercado Central", "12345678909", "11999990000", "a@b.com", ativo: false);

        Assert.Equal("Mercado Central", cliente.Nome);
        Assert.Equal("12345678909", cliente.Documento);
        Assert.False(cliente.Ativo);
    }
}
