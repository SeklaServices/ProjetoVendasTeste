using Vendas.Shared.Excecoes;

namespace Vendas.Host.Middleware;

/// <summary>
/// Traduz <see cref="RegraDeNegocioExcecao"/> em HTTP 422 num lugar só.
///
/// Sem isto, cada endpoint precisaria de um try/catch idêntico — e bastaria alguém esquecer um
/// para a API devolver 500 numa situação perfeitamente prevista.
/// </summary>
public sealed class TratamentoDeExcecoes(RequestDelegate proximo, ILogger<TratamentoDeExcecoes> log)
{
    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await proximo(contexto);
        }
        catch (RegraDeNegocioExcecao ex)
        {
            log.LogInformation("Regra de negócio violada: {Codigo} — {Mensagem}", ex.Codigo, ex.Message);

            contexto.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
            await contexto.Response.WriteAsJsonAsync(new { codigo = ex.Codigo, mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Erro não tratado ao processar {Metodo} {Caminho}",
                contexto.Request.Method, contexto.Request.Path);

            contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await contexto.Response.WriteAsJsonAsync(new
            {
                codigo = "ERRO_INTERNO",
                mensagem = "Ocorreu um erro inesperado. Consulte os logs do servidor.",
            });
        }
    }
}
