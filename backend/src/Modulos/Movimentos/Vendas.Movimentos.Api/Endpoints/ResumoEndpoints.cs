using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vendas.Movimentos.Application.Queries;

namespace Vendas.Movimentos.Api.Endpoints;

public static class ResumoEndpoints
{
    public static IEndpointRouteBuilder MapResumoEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/resumo", async (
                ResumoQueryHandler handler,
                DateOnly? dataInicial,
                DateOnly? dataFinal,
                CancellationToken ct) =>
                Results.Ok(await handler.HandleAsync(new ResumoQuery(dataInicial, dataFinal), ct)))
            .WithTags("Resumo");

        return app;
    }
}
