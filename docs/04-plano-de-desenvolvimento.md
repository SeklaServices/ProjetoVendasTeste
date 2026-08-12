# 04 — Plano de Desenvolvimento

**Status:** proposta para validação
**Versão:** 1.0

Este documento lista, na ordem, **tudo que será construído** e em qual branch/PR. Cada linha vira
uma issue no GitHub.

---

## Como este plano foi montado

O trabalho foi fatiado em PRs pequenos e com uma intenção: **cada PR ensina alguma coisa do fluxo
de git**, além de entregar código. Backend e frontend de um mesmo assunto são PRs separados — assim
duas pessoas podem trabalhar em paralelo e, de quebra, aparecem conflitos reais para resolver.

Alvo: 8 PRs até a `v1.0.0`.

---

## Etapa 0 — Fundação do repositório

**Não é PR.** É o commit inicial em `main`, feito uma única vez.

| Item | Conteúdo |
|---|---|
| `README.md`, `CLAUDE.md`, `docs/` | Esta documentação |
| `.gitignore`, `.gitattributes` | Node, .NET, Visual Studio, `appsettings.Development.json`, `*.db` |
| `.github/pull_request_template.md` | Template de PR |
| `.github/workflows/ci.yml` | CI: job `backend` (build + test) e job `frontend` (type-check + build) |

Depois: `develop` criada a partir de `main`, branch protection ligada nas duas.

---

## PR 1 — Esqueleto da solução

- **Branch:** `feature/1-esqueleto-solucao` → `develop`
- **Entrega:**
  - Solução `.slnx` com os 10 projetos (Host, Shared, 4 de Cadastros, 4 de Movimentos) e os 2 de teste
  - `Program.cs` do Host com Swagger, CORS e endpoint `GET /health`
  - Frontend Vite + React 19 + TS strict + AntD 6, com `LayoutPrincipal` e menu lateral (rotas vazias)
  - `dotnet build` e `npm run build` verdes no CI
- **Lição de git:** primeiro PR do repositório, primeiro CI rodando, primeira aprovação.

## PR 2 — Backend de Produtos

- **Branch:** `feature/2-produtos-backend` → `develop`
- **Entrega:**
  - Entidade `Produto` com regras (código obrigatório e único, preços não negativos)
  - `IProdutoRepositorio`, `CadastrosDbContext`, mapeamento, **primeira migration**
  - Commands: `Criar`, `Atualizar`, `Excluir` / Query: `ListarProdutos`, `ObterProduto`
  - Endpoints `/api/v1/produtos` (5 rotas)
  - Testes unitários das regras
- **Lição de git:** PR que toca as 4 camadas — review por camada.

## PR 3 — Tela de Produtos

- **Branch:** `feature/3-produtos-frontend` → `develop`
- **Entrega:** `produtosApi.ts`, `PaginaProdutos` (Table + busca), `ModalProduto` (Form),
  exclusão com confirmação, feedback com `message`, integração TanStack Query
- **Lição de git:** trabalho em paralelo com o PR 2 — depende dele, e vai precisar de `rebase`.

## PR 4 — Backend de Compras

- **Branch:** `feature/4-compras-backend` → `develop`
- **Entrega:**
  - Entidades `Compra` e `CompraItem`, cálculo de subtotal e total no domínio
  - Regras: mínimo 1 item, quantidade > 0, data não futura, produto ativo
  - `MovimentosDbContext`, migration, `SEQUENCE` do número
  - `IConsultaProdutos` em `Shared`, implementada em `Cadastros` — a comunicação entre módulos
  - Endpoints `/api/v1/compras`
  - Testes unitários das regras
- **Lição de git:** PR grande demais? É o candidato natural a ser quebrado em dois — bom exercício.

## PR 5 — Tela de Compras

- **Branch:** `feature/5-compras-frontend` → `develop`
- **Entrega:** lista com filtro de datas, modal de lançamento com grid de itens editável, tela de
  detalhe, totalizador reativo
- **Lição de git:** conflito provável em `Roteador.tsx` e no menu com o PR 3 — **resolver conflito de verdade**.

## PR 6 — Backend de Vendas

- **Branch:** `feature/6-vendas-backend` → `develop`
- **Entrega:** espelho de compras, com `Cliente` no lugar de `Fornecedor` e sugestão de preço de venda
- **Lição de git:** o PR que revela duplicação. A discussão "generalizar ou duplicar?" acontece no
  review — e a decisão vira registro no PR.

## PR 7 — Tela de Vendas

- **Branch:** `feature/7-vendas-frontend` → `develop`
- **Entrega:** espelho da tela de compras
- **Lição de git:** aproveitar componentes do PR 5 — refatorar código de outra pessoa com cuidado.

## PR 8 — Tela de Resumo

- **Branch:** `feature/8-resumo` → `develop`
- **Entrega:** endpoint `GET /api/v1/resumo` + tela com cards de totais, filtro de período e as
  últimas movimentações
- **Lição de git:** último PR antes da release.

---

## Release 1.0.0

- **Branch:** `release/1.0.0` → `main`
- **Entrega:** `CHANGELOG.md`, versão nos projetos, merge commit em `main`, tag `v1.0.0`, merge de
  volta em `develop`
- **Lição de git:** ciclo completo de release e tag.

---

## Depois da v1.0.0 — trabalho para a equipe

A partir daqui **eu paro e a equipe assume**. Os itens abaixo estão detalhados em
`06-exercicios-git.md` e existem para ser feitos por pessoas, não por IA:

| Issue | Tarefa | Exercita |
|---|---|---|
| Bug plantado | "Valor total da venda ignora o último item" | `hotfix/*` + tag `v1.0.1` |
| Cadastro de clientes | Substituir texto livre por cadastro | Feature completa ponta a ponta |
| Cadastro de fornecedores | Idem, por outra pessoa, **ao mesmo tempo** | Conflito real entre duas features |
| Exportar CSV | Botão nas listagens | PR pequeno, review rápido |
| Gráfico no resumo | Adicionar biblioteca de gráficos | `chore(deps)` + discussão de dependência |
| Login simples | JWT sem RBAC | Feature que atravessa todo o sistema |
| Edição de compra/venda | Mudança de decisão de escopo | Como reverter uma decisão documentada |

---

## Definition of Done (por PR)

- [ ] Faz o que a issue pediu
- [ ] Regra de negócio no Domain/Application — nunca em endpoint ou repositório
- [ ] Nenhum módulo acessa o `DbContext` de outro
- [ ] Testes unitários para toda regra nova
- [ ] `dotnet build` e `dotnet test` verdes localmente
- [ ] `npm run type-check` e `npm run build` verdes localmente
- [ ] Sem segredo ou string de conexão no código
- [ ] Descrição do PR preenchida com **o quê / por quê / como testar / decisões**
- [ ] CI verde
- [ ] 1 aprovação
- [ ] Termo de negócio novo adicionado ao glossário (`01-escopo-funcional.md` §5)
