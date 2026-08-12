using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Vendas.Cadastros.Api.Endpoints;
using Vendas.Cadastros.Application;
using Vendas.Cadastros.Infrastructure;
using Vendas.Cadastros.Infrastructure.Persistencia;
using Vendas.Host.Middleware;
using Vendas.Movimentos.Api.Endpoints;
using Vendas.Movimentos.Application;
using Vendas.Movimentos.Infrastructure;
using Vendas.Movimentos.Infrastructure.Persistencia;

var construtor = WebApplication.CreateBuilder(args);

// A string de conexão vem de appsettings.Development.json — que NÃO é versionado.
// Cada desenvolvedor tem o seu banco local (ver docs/07-decisoes.md D-001).
var stringDeConexao = construtor.Configuration.GetConnectionString("Padrao")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:Padrao não configurada. Copie appsettings.Development.example.json "
        + "para appsettings.Development.json e ajuste a instância do SQL Server.");

construtor.Services
    .AdicionarCadastrosApplication()
    .AdicionarCadastrosInfrastructure(stringDeConexao)
    .AdicionarMovimentosApplication()
    .AdicionarMovimentosInfrastructure(stringDeConexao);

construtor.Services.AddOpenApi();

// CORS para o Vite em desenvolvimento. Qualquer porta de localhost é aceita porque o Vite pula
// para 5174, 5175… quando a 5173 está ocupada — e ninguém deveria perder tempo com isso.
// Num sistema real a lista de origens seria fixa e viria de configuração por ambiente.
const string PoliticaCors = "frontend-local";
construtor.Services.AddCors(opcoes => opcoes.AddPolicy(PoliticaCors, politica => politica
    .SetIsOriginAllowed(origem =>
        Uri.TryCreate(origem, UriKind.Absolute, out var uri)
        && (uri.Host == "localhost" || uri.Host == "127.0.0.1"))
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = construtor.Build();

// Aplica as migrations no boot. Aceitável num projeto de treino com banco local; num sistema real
// isso é papel do pipeline de deploy, não da aplicação.
if (app.Environment.IsDevelopment())
{
    using var escopo = app.Services.CreateScope();
    await escopo.ServiceProvider.GetRequiredService<CadastrosDbContext>().Database.MigrateAsync();
    await escopo.ServiceProvider.GetRequiredService<MovimentosDbContext>().Database.MigrateAsync();
}

app.UseMiddleware<TratamentoDeExcecoes>();
app.UseCors(PoliticaCors);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/health", () => Results.Ok(new { situacao = "ok" })).WithTags("Infraestrutura");

app.MapProdutosEndpoints();
app.MapComprasEndpoints();
app.MapVendasEndpoints();
app.MapResumoEndpoints();

app.Run();
