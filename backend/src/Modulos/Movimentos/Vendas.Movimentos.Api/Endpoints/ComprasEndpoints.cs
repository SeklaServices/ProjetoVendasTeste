using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vendas.Movimentos.Application.Commands;
using Vendas.Movimentos.Application.Queries;

namespace Vendas.Movimentos.Api.Endpoints;

public static class ComprasEndpoints
{
    public static IEndpointRouteBuilder MapComprasEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/compras").WithTags("Compras");

        grupo.MapGet("/", async (
            ListarComprasQueryHandler handler,
            DateOnly? dataInicial,
            DateOnly? dataFinal,
            CancellationToken ct) =>
            Results.Ok(await handler.HandleAsync(new ListarComprasQuery(dataInicial, dataFinal), ct)));

        grupo.MapGet("/{id:guid}", async (
            Guid id, ObterCompraQueryHandler handler, CancellationToken ct) =>
        {
            var compra = await handler.HandleAsync(id, ct);
            return compra is null ? Results.NotFound() : Results.Ok(compra);
        });

        grupo.MapPost("/", async (
            CriarCompraReq req, CriarCompraCommandHandler handler, CancellationToken ct) =>
        {
            // O valor total NÃO vem do request — é sempre calculado pelo domínio a partir dos itens.
            var itens = (req.Itens ?? [])
                .Select(i => new ItemCommand(i.ProdutoId, i.Quantidade, i.PrecoUnitario))
                .ToList();

            var (id, numero) = await handler.HandleAsync(
                new CriarCompraCommand(req.Data, req.Fornecedor, req.Observacao, itens), ct);

            return Results.Created($"/api/v1/compras/{id}", new { id, numero });
        });

        grupo.MapDelete("/{id:guid}", async (
            Guid id, ExcluirCompraCommandHandler handler, CancellationToken ct) =>
        {
            var encontrada = await handler.HandleAsync(id, ct);
            return encontrada ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }

    private sealed record ItemReq(Guid ProdutoId, decimal Quantidade, decimal PrecoUnitario);

    private sealed record CriarCompraReq(
        DateOnly Data, string Fornecedor, string? Observacao, IReadOnlyList<ItemReq>? Itens);
}
