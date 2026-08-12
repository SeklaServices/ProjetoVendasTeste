using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vendas.Cadastros.Application.Commands;
using Vendas.Cadastros.Application.Queries;

namespace Vendas.Cadastros.Api.Endpoints;

/// <summary>
/// Endpoints de clientes. Como em produtos: recebe o request, chama o handler, traduz em HTTP.
/// Nenhuma decisão de negócio aqui — o 422 vem do middleware do Host.
/// </summary>
public static class ClientesEndpoints
{
    public static IEndpointRouteBuilder MapClientesEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/clientes").WithTags("Clientes");

        grupo.MapGet("/", async (
            ListarClientesQueryHandler handler,
            string? busca,
            bool? apenasAtivos,
            CancellationToken ct) =>
            Results.Ok(await handler.HandleAsync(
                new ListarClientesQuery(busca, apenasAtivos ?? false), ct)));

        grupo.MapGet("/{id:guid}", async (
            Guid id, ObterClienteQueryHandler handler, CancellationToken ct) =>
        {
            var cliente = await handler.HandleAsync(id, ct);
            return cliente is null ? Results.NotFound() : Results.Ok(cliente);
        });

        // O código não vem no corpo em nenhuma das rotas: é gerado pelo banco e não se altera.
        grupo.MapPost("/", async (
            CriarClienteReq req, CriarClienteCommandHandler handler, CancellationToken ct) =>
        {
            var id = await handler.HandleAsync(
                new CriarClienteCommand(req.Nome, req.Documento, req.Telefone, req.Email), ct);

            return Results.Created($"/api/v1/clientes/{id}", new { id });
        });

        grupo.MapPut("/{id:guid}", async (
            Guid id, AtualizarClienteReq req, AtualizarClienteCommandHandler handler, CancellationToken ct) =>
        {
            var encontrado = await handler.HandleAsync(
                new AtualizarClienteCommand(id, req.Nome, req.Documento, req.Telefone, req.Email, req.Ativo),
                ct);

            return encontrado ? Results.NoContent() : Results.NotFound();
        });

        grupo.MapDelete("/{id:guid}", async (
            Guid id, ExcluirClienteCommandHandler handler, CancellationToken ct) =>
        {
            var encontrado = await handler.HandleAsync(id, ct);
            return encontrado ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }

    private sealed record CriarClienteReq(string Nome, string? Documento, string? Telefone, string? Email);

    private sealed record AtualizarClienteReq(
        string Nome, string? Documento, string? Telefone, string? Email, bool Ativo);
}
