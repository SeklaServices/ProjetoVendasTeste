# 02 — Arquitetura

**Status:** proposta para validação
**Versão:** 1.0

Este documento define a arquitetura do ProjetoVendasTeste. Ela é uma **versão reduzida e fiel** da
arquitetura do `CeasaSystemNext`, para que a prática de git aqui se pareça com o trabalho real lá.

---

## 1. Princípio: espelhar o real, em escala pequena

| CeasaSystemNext (real) | ProjetoVendasTeste (treino) |
|---|---|
| .NET 10, Minimal API, sem Controllers | ✅ Igual |
| Clean Architecture por módulo (Domain/Application/Infrastructure/Api) | ✅ Igual, com **2 módulos** |
| 20 módulos | **2 módulos**: `Cadastros`, `Movimentos` |
| EF Core (escrita) + Dapper (leitura) | **Só EF Core** — Dapper é complexidade sem retorno aqui |
| Nomes de pastas, classes e campos em **português** | ✅ Igual |
| Multi-empresa, multi-filial, RBAC, Outbox | ❌ Nada disso |
| React 19 + TS strict + Vite + AntD 6 + TanStack Query + Axios | ✅ Igual |
| `src/modulos/{modulo}/{paginas,componentes,servicos}` | ✅ Igual |
| GitHub Actions (backend + frontend) | ✅ Igual, mais simples |

---

## 2. Estrutura de diretórios

```
ProjetoVendasTeste/
├── README.md
├── CLAUDE.md                              ← regras do projeto
├── .gitignore
├── .gitattributes
├── docs/                                  ← esta documentação
├── .github/
│   ├── workflows/ci.yml                   ← build + testes (backend e frontend)
│   └── pull_request_template.md           ← template de PR (usado em todo PR)
│
├── backend/
│   ├── ProjetoVendasTeste.slnx
│   ├── src/
│   │   ├── Host/
│   │   │   └── Vendas.Host/               ← Program.cs, appsettings, composição
│   │   ├── Compartilhado/
│   │   │   └── Vendas.Shared/             ← exceções, tipos e helpers comuns
│   │   └── Modulos/
│   │       ├── Cadastros/
│   │       │   ├── Vendas.Cadastros.Domain/
│   │       │   ├── Vendas.Cadastros.Application/
│   │       │   ├── Vendas.Cadastros.Infrastructure/
│   │       │   └── Vendas.Cadastros.Api/
│   │       └── Movimentos/
│   │           ├── Vendas.Movimentos.Domain/
│   │           ├── Vendas.Movimentos.Application/
│   │           ├── Vendas.Movimentos.Infrastructure/
│   │           └── Vendas.Movimentos.Api/
│   └── testes/
│       ├── Vendas.Cadastros.Tests.Unit/
│       └── Vendas.Movimentos.Tests.Unit/
│
└── frontend/
    ├── package.json, vite.config.ts, tsconfig.json
    └── src/
        ├── main.tsx, App.tsx
        ├── infraestrutura/api/cliente.ts   ← Axios configurado
        ├── roteador/Roteador.tsx
        ├── compartilhado/
        │   ├── layouts/LayoutPrincipal.tsx
        │   └── utils/formatadores.ts
        └── modulos/
            ├── resumo/paginas/PaginaResumo.tsx
            ├── produtos/{paginas,componentes,servicos}/
            ├── compras/{paginas,componentes,servicos}/
            └── vendas/{paginas,componentes,servicos}/
```

---

## 3. Camadas do backend

Regra de dependência — **as setas só apontam para dentro**:

```
Api  →  Application  →  Domain
              ↑
       Infrastructure
```

| Camada | Contém | Nunca contém |
|---|---|---|
| **Domain** | Entidades, regras de negócio, interfaces de repositório, exceções de domínio | EF Core, HTTP, JSON, nada de infraestrutura |
| **Application** | Commands e Queries (com seus Handlers), DTOs, validações de caso de uso | SQL, EF Core diretamente (usa a interface do repositório) |
| **Infrastructure** | `DbContext`, configurações de mapeamento, migrations, implementação dos repositórios | Regra de negócio |
| **Api** | Endpoints Minimal API — recebe o request, chama o handler, traduz o resultado em HTTP | Qualquer `if` de regra de negócio |

**A regra que mais será cobrada em code review:** se você escreveu um `if` que decide algo de
negócio dentro de um endpoint ou de um repositório, está no lugar errado.

### Padrão de Command/Query

Mesmo padrão do projeto real: um `record` para a entrada, uma classe `...Handler` com um método
`HandleAsync`. Sem MediatR — o handler é injetado direto no endpoint.

```csharp
public sealed record CriarProdutoCommand(
    string Codigo, string Nome, string UnidadeMedida,
    decimal PrecoCusto, decimal PrecoVenda);

public sealed class CriarProdutoCommandHandler(IProdutoRepositorio repositorio)
{
    public async Task<Guid> HandleAsync(CriarProdutoCommand comando, CancellationToken ct) { ... }
}
```

---

## 4. Modelo de dados

Cinco tabelas. Sem tabela de estoque — por decisão de escopo.

```
Produtos
  Id                uniqueidentifier  PK
  Codigo            nvarchar(20)      UNIQUE
  Nome              nvarchar(120)
  UnidadeMedida     nvarchar(10)
  PrecoCusto        decimal(18,4)
  PrecoVenda        decimal(18,4)
  Ativo             bit
  DataCadastro      datetime2

Clientes
  Id                uniqueidentifier  PK
  Codigo            int               UNIQUE (sequence SeqCliente) — gerado, não digitado
  Nome              nvarchar(120)
  Documento         nvarchar(18)      NULL, UNIQUE filtrado (WHERE Documento IS NOT NULL)
  Telefone          nvarchar(20)      NULL
  Email             nvarchar(120)     NULL
  Ativo             bit
  DataCadastro      datetime2

Compras                              Vendas
  Id       uniqueidentifier PK         Id        uniqueidentifier PK
  Numero   int UNIQUE (sequence)       Numero    int UNIQUE (sequence)
  Data     date                        Data      date
  Fornecedor nvarchar(120)             ClienteId uniqueidentifier (indexado, sem FK)
  Observacao nvarchar(500) NULL        Observacao nvarchar(500) NULL
  ValorTotal decimal(18,4)             ValorTotal decimal(18,4)
  DataCriacao datetime2                DataCriacao datetime2

ComprasItens                         VendasItens
  Id            uniqueidentifier PK    Id            uniqueidentifier PK
  CompraId      FK → Compras (cascade) VendaId       FK → Vendas (cascade)
  ProdutoId     indexado (sem FK)      ProdutoId     indexado (sem FK)
  Quantidade    decimal(18,4)          Quantidade    decimal(18,4)
  PrecoUnitario decimal(18,4)          PrecoUnitario decimal(18,4)
  Subtotal      decimal(18,4)          Subtotal      decimal(18,4)
```

**Decisões:**
- `Id` é `Guid` (gerado na aplicação) — igual ao projeto real.
- `Numero` vem de uma `SEQUENCE` do SQL Server (`SeqCompra`, `SeqVenda`) — igual ao projeto real.
- FK de item → documento é `CASCADE`: excluir a compra apaga seus itens.
- Item → produto **não tem FK**, só índice em `ProdutoId`. `Produtos` pertence ao DbContext de
  outro módulo, e uma FK entre migrations de módulos diferentes as tornaria dependentes uma da
  outra. A integridade fica na camada Application (`ExcluirProdutoCommandHandler` e a validação
  dos itens). Ver `07-decisoes.md` D-007.
- `ValorTotal` e `Subtotal` são **persistidos**, mas sempre calculados pelo domínio. A API rejeita
  qualquer valor enviado pelo cliente para esses campos.

### Um DbContext por módulo

`CadastrosDbContext` e `MovimentosDbContext`, cada um com suas próprias migrations, apontando para
o **mesmo banco físico**. Um módulo **nunca** acessa o DbContext do outro — regra herdada do
projeto real.

Consequência prática: `Movimentos` precisa saber se um produto existe e está ativo, mas não pode
ler `Produtos` pelo DbContext de `Cadastros`. A solução é uma interface em `Vendas.Shared`
(`IConsultaProdutos`), implementada em `Cadastros` e injetada em `Movimentos`. Isso é justamente o
tipo de decisão que gera boa discussão em code review.

---

## 5. Contratos de API

Base: `/api/v1`. Formato de erro padronizado: `{ "codigo": "...", "mensagem": "..." }`.

### Clientes

| Método | Rota | Retorno |
|---|---|---|
| `GET` | `/clientes?busca=&apenasAtivos=` | `200` lista de clientes |
| `GET` | `/clientes/{id}` | `200` cliente \| `404` |
| `POST` | `/clientes` | `201` + `{ id }` \| `422` |
| `PUT` | `/clientes/{id}` | `204` \| `404` \| `422` |
| `DELETE` | `/clientes/{id}` | `204` \| `422` (cliente com vendas) |

A busca aceita código, nome ou documento. O `codigo` nunca vem no corpo — é gerado pelo banco.

### Produtos

| Método | Rota | Retorno |
|---|---|---|
| `GET` | `/produtos?busca=&apenasAtivos=` | `200` lista de produtos |
| `GET` | `/produtos/{id}` | `200` produto \| `404` |
| `POST` | `/produtos` | `201` + `{ id }` \| `422` regra violada |
| `PUT` | `/produtos/{id}` | `204` \| `404` \| `422` |
| `DELETE` | `/produtos/{id}` | `204` \| `422` (produto em uso) |

### Compras

| Método | Rota | Retorno |
|---|---|---|
| `GET` | `/compras?dataInicial=&dataFinal=` | `200` lista (cabeçalhos) |
| `GET` | `/compras/{id}` | `200` compra com itens \| `404` |
| `POST` | `/compras` | `201` + `{ id, numero }` \| `422` |
| `DELETE` | `/compras/{id}` | `204` \| `404` |

### Vendas

Mesmas rotas de compras, trocando `/compras` por `/vendas`. A diferença está no parceiro: a compra
recebe `fornecedor` (texto livre) e a venda recebe **`clienteId`** (id do cadastro). As respostas da
venda trazem `clienteId`, `clienteCodigo` e `clienteNome` — o nome é resolvido do cadastro a cada
consulta, porque a venda guarda só o id (D-009).

### Resumo

| Método | Rota | Retorno |
|---|---|---|
| `GET` | `/resumo?dataInicial=&dataFinal=` | `200` totais + últimas movimentações |

### Códigos HTTP — convenção do projeto

| Código | Quando |
|---|---|
| `200` | Leitura bem-sucedida |
| `201` | Criação — com `Location` e o id no corpo |
| `204` | Atualização ou exclusão bem-sucedida, sem corpo |
| `404` | Recurso não encontrado |
| `422` | Regra de negócio violada (corpo válido, operação inválida) |
| `400` | Corpo malformado / falha de validação de formato |

---

## 6. Frontend

- **React 19 + TypeScript `strict`**. `any` é reprovado em review.
- **Ant Design 6** para tudo: `Table`, `Form`, `Modal`, `InputNumber`, `DatePicker`, `Select`,
  `message` para feedback.
- **TanStack Query v5** para todo acesso a servidor. Nada de `useEffect` + `fetch` manual.
- **Axios** com instância única em `infraestrutura/api/cliente.ts` (`baseURL` de variável de
  ambiente `VITE_API_URL`).
- Cada módulo expõe seu `servicos/{modulo}Api.ts` com as funções tipadas — mesmo padrão do
  `metasApi.ts` do projeto real.

### Telas

| Rota | Tela | Conteúdo |
|---|---|---|
| `/` | Resumo | Cards de totais + últimas movimentações |
| `/produtos` | Produtos | `Table` com busca + `Modal` de cadastro/edição |
| `/clientes` | Clientes | `Table` com busca (código, nome ou documento) + `Modal` de cadastro/edição |
| `/compras` | Compras | `Table` com filtro de data + `Modal` de lançamento (cabeçalho + grid de itens) |
| `/compras/:id` | Detalhe da compra | Somente leitura |
| `/vendas` | Vendas | Igual a compras |
| `/vendas/:id` | Detalhe da venda | Somente leitura |

---

## 7. Testes

Escopo enxuto e honesto: **testes unitários de domínio e de handler**, com xUnit + NSubstitute.

Sem testes de integração com banco e sem E2E — no projeto real eles existem, mas aqui custariam
mais setup do que ensinam sobre git. O que precisa existir é um `dotnet test` que roda rápido e
**fica vermelho quando alguém quebra uma regra** — porque o CI vermelho bloqueando um PR é parte do
que estamos treinando.

Exemplos do que será testado:
- Compra sem itens é rejeitada
- Quantidade zero ou negativa é rejeitada
- `ValorTotal` = soma dos subtotais
- Data futura é rejeitada
- Código de produto duplicado é rejeitado
