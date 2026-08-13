using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vendas.Cadastros.Application.Commands;
using Vendas.Cadastros.Application.Queries;

namespace Vendas.Cadastros.Api.Endpoints;

/// <summary>
/// Endpoints de produtos.
///
/// Um endpoint faz três coisas: recebe o request, chama o handler, traduz o resultado em HTTP.
/// Nenhuma decisão de negócio acontece aqui — a exceção <c>RegraDeNegocioExcecao</c> é traduzida
/// em 422 pelo middleware do Host, não por try/catch em cada rota.
/// </summary>
public static class ProdutosEndpoints
{
    public static IEndpointRouteBuilder MapProdutosEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/produtos").WithTags("Produtos");

        grupo.MapGet("/", async (
            ListarProdutosQueryHandler handler,
            string? busca,
            bool? apenasAtivos,
            CancellationToken ct) =>
            Results.Ok(await handler.HandleAsync(
                new ListarProdutosQuery(busca, apenasAtivos ?? false), ct)));

        grupo.MapGet("/{id:guid}", async (
            Guid id, ObterProdutoQueryHandler handler, CancellationToken ct) =>
        {
            var produto = await handler.HandleAsync(id, ct);
            return produto is null ? Results.NotFound() : Results.Ok(produto);
        });

        grupo.MapPost("/", async (
            CriarProdutoReq req, CriarProdutoCommandHandler handler, CancellationToken ct) =>
        {
            var id = await handler.HandleAsync(
                new CriarProdutoCommand(
                    req.Codigo, req.Nome, req.UnidadeMedida, req.PrecoCusto, req.PrecoVenda),
                ct);

            return Results.Created($"/api/v1/produtos/{id}", new { id });
        });

        grupo.MapPut("/{id:guid}", async (
            Guid id, AtualizarProdutoReq req, AtualizarProdutoCommandHandler handler, CancellationToken ct) =>
        {
            var encontrado = await handler.HandleAsync(
                new AtualizarProdutoCommand(
                    id, req.Codigo, req.Nome, req.UnidadeMedida, req.PrecoCusto, req.PrecoVenda, req.Ativo),
                ct);

            return encontrado ? Results.NoContent() : Results.NotFound();
        });

        grupo.MapDelete("/{id:guid}", async (
            Guid id, ExcluirProdutoCommandHandler handler, CancellationToken ct) =>
        {
            var encontrado = await handler.HandleAsync(id, ct);
            return encontrado ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }

    private sealed record CriarProdutoReq(
        string Codigo, string Nome, string UnidadeMedida, decimal PrecoCusto, decimal PrecoVenda);

    private sealed record AtualizarProdutoReq(
        string Codigo, string Nome, string UnidadeMedida, decimal PrecoCusto, decimal PrecoVenda, bool Ativo);
}
