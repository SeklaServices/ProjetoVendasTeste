using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vendas.Movimentos.Application.Commands;
using Vendas.Movimentos.Application.Queries;

namespace Vendas.Movimentos.Api.Endpoints;

public static class VendasEndpoints
{
    public static IEndpointRouteBuilder MapVendasEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/vendas").WithTags("Vendas");

        grupo.MapGet("/", async (
            ListarVendasQueryHandler handler,
            DateOnly? dataInicial,
            DateOnly? dataFinal,
            CancellationToken ct) =>
            Results.Ok(await handler.HandleAsync(new ListarVendasQuery(dataInicial, dataFinal), ct)));

        grupo.MapGet("/{id:guid}", async (
            Guid id, ObterVendaQueryHandler handler, CancellationToken ct) =>
        {
            var venda = await handler.HandleAsync(id, ct);
            return venda is null ? Results.NotFound() : Results.Ok(venda);
        });

        grupo.MapPost("/", async (
            CriarVendaReq req, CriarVendaCommandHandler handler, CancellationToken ct) =>
        {
            var itens = (req.Itens ?? [])
                .Select(i => new ItemCommand(i.ProdutoId, i.Quantidade, i.PrecoUnitario))
                .ToList();

            var (id, numero) = await handler.HandleAsync(
                new CriarVendaCommand(req.Data, req.ClienteId, req.Observacao, itens), ct);

            return Results.Created($"/api/v1/vendas/{id}", new { id, numero });
        });

        grupo.MapDelete("/{id:guid}", async (
            Guid id, ExcluirVendaCommandHandler handler, CancellationToken ct) =>
        {
            var encontrada = await handler.HandleAsync(id, ct);
            return encontrada ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }

    private sealed record ItemReq(Guid ProdutoId, decimal Quantidade, decimal PrecoUnitario);

    /// <param name="ClienteId">Id do cliente cadastrado. O nome não trafega mais — ver D-009.</param>
    private sealed record CriarVendaReq(
        DateOnly Data, Guid ClienteId, string? Observacao, IReadOnlyList<ItemReq>? Itens);
}
