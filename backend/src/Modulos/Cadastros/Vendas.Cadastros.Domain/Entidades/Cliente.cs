using Vendas.Shared.Excecoes;

namespace Vendas.Cadastros.Domain.Entidades;

/// <summary>
/// Cliente do cadastro. Substitui o texto livre que existia no campo Cliente da venda.
///
/// O código é sequencial e gerado pelo banco — o usuário não digita e não pode alterar
/// (docs/07-decisoes.md D-009).
/// </summary>
public sealed class Cliente
{
    public const int TamanhoMaximoNome = 120;
    public const int TamanhoMaximoDocumento = 18;
    public const int TamanhoMaximoTelefone = 20;
    public const int TamanhoMaximoEmail = 120;

    private Cliente() { }

    private Cliente(string nome, string? documento, string? telefone, string? email)
    {
        Id = Guid.NewGuid();
        DataCadastro = DateTime.UtcNow;
        Ativo = true;
        AplicarDados(nome, documento, telefone, email);
    }

    public Guid Id { get; private set; }

    /// <summary>Sequencial, gerado pelo banco (SEQUENCE). Só tem valor depois de salvo.</summary>
    public int Codigo { get; private set; }

    public string Nome { get; private set; } = string.Empty;

    /// <summary>CPF ou CNPJ, só dígitos. Opcional, mas único quando informado.</summary>
    public string? Documento { get; private set; }

    public string? Telefone { get; private set; }
    public string? Email { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime DataCadastro { get; private set; }

    public static Cliente Criar(string nome, string? documento, string? telefone, string? email)
        => new(nome, documento, telefone, email);

    public void Atualizar(string nome, string? documento, string? telefone, string? email, bool ativo)
    {
        AplicarDados(nome, documento, telefone, email);
        Ativo = ativo;
    }

    /// <summary>
    /// Deixa o documento só com dígitos. "12.345.678/0001-90" e "12345678000190" são o mesmo
    /// documento — guardar formatado faria os dois passarem pela verificação de duplicidade.
    /// </summary>
    public static string? NormalizarDocumento(string? documento)
    {
        if (string.IsNullOrWhiteSpace(documento))
        {
            return null;
        }

        var digitos = new string(documento.Where(char.IsDigit).ToArray());
        return digitos.Length == 0 ? null : digitos;
    }

    private void AplicarDados(string nome, string? documento, string? telefone, string? email)
    {
        nome = (nome ?? string.Empty).Trim();
        documento = NormalizarDocumento(documento);
        telefone = string.IsNullOrWhiteSpace(telefone) ? null : telefone.Trim();
        email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();

        if (nome.Length == 0)
        {
            throw new RegraDeNegocioExcecao("CLIENTE_NOME_OBRIGATORIO", "Informe o nome do cliente.");
        }

        if (nome.Length > TamanhoMaximoNome)
        {
            throw new RegraDeNegocioExcecao(
                "CLIENTE_NOME_LONGO", $"O nome deve ter no máximo {TamanhoMaximoNome} caracteres.");
        }

        // Só o tamanho é verificado: CPF tem 11 dígitos, CNPJ tem 14. O dígito verificador NÃO é
        // validado — decisão registrada em D-009, com a limitação assumida.
        if (documento is not null && documento.Length is not (11 or 14))
        {
            throw new RegraDeNegocioExcecao(
                "CLIENTE_DOCUMENTO_INVALIDO",
                "O documento deve ser um CPF (11 dígitos) ou um CNPJ (14 dígitos).");
        }

        if (telefone is { Length: > TamanhoMaximoTelefone })
        {
            throw new RegraDeNegocioExcecao(
                "CLIENTE_TELEFONE_LONGO", $"O telefone deve ter no máximo {TamanhoMaximoTelefone} caracteres.");
        }

        if (email is not null)
        {
            if (email.Length > TamanhoMaximoEmail)
            {
                throw new RegraDeNegocioExcecao(
                    "CLIENTE_EMAIL_LONGO", $"O e-mail deve ter no máximo {TamanhoMaximoEmail} caracteres.");
            }

            // Verificação de formato deliberadamente simples: validar e-mail "de verdade" com regex
            // rejeita endereços válidos e aceita inválidos. O que pega erro de digitação de fato é
            // exigir um arroba no meio, um ponto no domínio depois dele, e nenhum espaço.
            //
            // Consequência assumida: endereços de intranet sem ponto ("fulano@servidor") são
            // recusados. Num cadastro de clientes isso quase sempre é erro de digitação.
            var arroba = email.IndexOf('@');
            var pontoNoDominio = email.LastIndexOf('.');

            var formatoValido = arroba > 0
                && pontoNoDominio > arroba + 1
                && pontoNoDominio < email.Length - 1
                && !email.Contains(' ');

            if (!formatoValido)
            {
                throw new RegraDeNegocioExcecao("CLIENTE_EMAIL_INVALIDO", "O e-mail informado não é válido.");
            }
        }

        Nome = nome;
        Documento = documento;
        Telefone = telefone;
        Email = email;
    }
}
