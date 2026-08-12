using Vendas.Movimentos.Domain.Entidades;
using Vendas.Movimentos.Domain.Interfaces;
using Vendas.Shared.Contratos;

namespace Vendas.Movimentos.Application.Commands;

public sealed record CriarVendaCommand(
    DateOnly Data,
    string Cliente,
    string? Observacao,
    IReadOnlyList<ItemCommand> Itens);

public sealed class CriarVendaCommandHandler(
    IVendaRepositorio repositorio,
    IConsultaProdutos consultaProdutos)
{
    public async Task<(Guid Id, int Numero)> HandleAsync(CriarVendaCommand comando, CancellationToken ct)
    {
        var itensInformados = comando.Itens ?? [];

        await ValidacaoProdutosDosItens.GarantirProdutosValidosAsync(
            consultaProdutos, itensInformados.Select(i => i.ProdutoId).ToList(), ct);

        var itens = itensInformados
            .Select(i => VendaItem.Criar(i.ProdutoId, i.Quantidade, i.PrecoUnitario))
            .ToList();

        var venda = Venda.Criar(comando.Data, comando.Cliente, comando.Observacao, itens);

        await repositorio.AdicionarAsync(venda, ct);
        await repositorio.SalvarAsync(ct);

        return (venda.Id, venda.Numero);
    }
}
