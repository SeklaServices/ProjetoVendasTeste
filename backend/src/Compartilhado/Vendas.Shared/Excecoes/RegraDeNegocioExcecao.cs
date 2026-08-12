namespace Vendas.Shared.Excecoes;

/// <summary>
/// Regra de negócio violada. O corpo do request era válido, mas a operação não pode acontecer.
/// A camada Api traduz esta exceção em HTTP 422 — nenhum endpoint decide isso por conta própria.
/// </summary>
public sealed class RegraDeNegocioExcecao(string codigo, string mensagem) : Exception(mensagem)
{
    /// <summary>Identificador estável da regra, para o frontend reagir sem depender do texto.</summary>
    public string Codigo { get; } = codigo;
}
