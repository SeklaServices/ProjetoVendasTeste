namespace Vendas.Shared;

/// <summary>
/// Identificadores fixos, criados por migration e conhecidos pelos dois módulos.
///
/// Existem porque a migration de Movimentos precisa referenciar um cliente criado pela migration
/// de Cadastros, e as duas rodam em contextos separados — um id constante é a única forma de
/// ligá-las sem uma consultar a tabela da outra.
/// </summary>
public static class ClientesConhecidos
{
    /// <summary>
    /// "Cliente não identificado". Recebe as vendas que existiam antes de o cadastro de clientes
    /// existir, quando o cliente era apenas um texto digitado. Ver docs/07-decisoes.md D-009.
    /// </summary>
    public static readonly Guid NaoIdentificado = new("9e1d0000-0000-4000-8000-000000000001");
}
