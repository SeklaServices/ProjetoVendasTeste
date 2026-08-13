using Vendas.Movimentos.Domain.Entidades;
using Vendas.Movimentos.Domain.Interfaces;
using Vendas.Shared.Contratos;
using Vendas.Shared.Excecoes;

namespace Vendas.Movimentos.Application.Commands;

public sealed record CriarVendaCommand(
    DateOnly Data,
    Guid ClienteId,
    string? Observacao,
    IReadOnlyList<ItemCommand> Itens);

public sealed class CriarVendaCommandHandler(
    IVendaRepositorio repositorio,
    IConsultaProdutos consultaProdutos,
    IConsultaClientes consultaClientes)
{
    public async Task<(Guid Id, int Numero)> HandleAsync(CriarVendaCommand comando, CancellationToken ct)
    {
        var itensInformados = comando.Itens ?? [];

        await GarantirClienteValidoAsync(comando.ClienteId, ct);

        await ValidacaoProdutosDosItens.GarantirProdutosValidosAsync(
            consultaProdutos, itensInformados.Select(i => i.ProdutoId).ToList(), ct);

        var itens = itensInformados
            .Select(i => VendaItem.Criar(i.ProdutoId, i.Quantidade, i.PrecoUnitario))
            .ToList();

        var venda = Venda.Criar(comando.Data, comando.ClienteId, comando.Observacao, itens);

        await repositorio.AdicionarAsync(venda, ct);
        await repositorio.SalvarAsync(ct);

        return (venda.Id, venda.Numero);
    }

    /// <summary>
    /// O cliente precisa existir e estar ativo. É regra de caso de uso, não de entidade: quem sabe
    /// disso é o módulo Cadastros, consultado pelo contrato — nunca pelo DbContext dele.
    /// </summary>
    private async Task GarantirClienteValidoAsync(Guid clienteId, CancellationToken ct)
    {
        if (clienteId == Guid.Empty)
        {
            // Deixa a entidade recusar, para a mensagem sair de um lugar só.
            return;
        }

        var encontrados = await consultaClientes.ObterPorIdsAsync([clienteId], ct);
        var cliente = encontrados.FirstOrDefault();

        if (cliente is null)
        {
            throw new RegraDeNegocioExcecao(
                "VENDA_CLIENTE_INEXISTENTE", "O cliente informado não existe.");
        }

        if (!cliente.Ativo)
        {
            throw new RegraDeNegocioExcecao(
                "VENDA_CLIENTE_INATIVO",
                $"O cliente '{cliente.Codigo} — {cliente.Nome}' está inativo e não pode receber vendas.");
        }
    }
}
