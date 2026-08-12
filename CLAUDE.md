# CLAUDE.md — ProjetoVendasTeste

**Versão:** 1.0
**Status:** Normativo — leitura obrigatória para qualquer IA e qualquer desenvolvedor

> Em caso de conflito entre este arquivo e qualquer outro documento, este prevalece.

---

## 1. O que é este projeto

Sistema web simples de vendas — cadastro de produtos, entradas de compra e saídas de venda —
construído para servir de **campo de treino do fluxo de trabalho com Git e GitHub** no escritório.

**O produto real deste projeto é a prática do fluxo, não o software.** Toda decisão técnica se
subordina a isso: se algo torna o sistema mais completo mas o fluxo mais confuso, não entra.

**O que este projeto não é:**
- Não é um ERP, nem um protótipo de um
- Não controla estoque (decisão explícita do responsável)
- Não vai para produção real
- Não é lugar para experimentar arquitetura

**Referência:** a arquitetura é uma redução fiel do `C:\ProjetosClaude\CeasaSystemNext`. Quando
houver dúvida sobre "como se faz aqui", a resposta é "como se faz lá, só que menor".

---

## 2. Stack oficial

| Camada | Tecnologia | Observação |
|---|---|---|
| Runtime | **.NET 10** | Minimal API. **Sem Controllers MVC.** |
| ORM | **EF Core 10** | Code-first, migrations por módulo. Sem Dapper aqui. |
| Banco | **SQL Server** local | Um banco por desenvolvedor. |
| Testes | **xUnit + NSubstitute** | Unitários apenas. |
| Frontend | **React 19 + TypeScript strict** | `any` é reprovado em review. |
| Build front | **Vite** | |
| UI | **Ant Design 6** | Não misturar outra biblioteca de componentes. |
| Estado servidor | **TanStack Query v5** | Sem `useEffect` + `fetch` manual. |
| HTTP | **Axios** | Instância única em `infraestrutura/api/cliente.ts`. |
| CI | **GitHub Actions** | |

Adicionar dependência nova exige PR próprio, tipo `chore(deps)`, com justificativa na descrição.

---

## 3. Arquitetura

- **Monólito modular**, 2 módulos: `Cadastros` (produtos) e `Movimentos` (compras e vendas)
- **Clean Architecture** por módulo: `Domain ← Application ← Infrastructure`, `Api` na borda
- **Um `DbContext` por módulo**, no mesmo banco físico
- Comunicação entre módulos apenas por interface em `Vendas.Shared` — nunca por `DbContext`
- Nomes de pastas, classes, propriedades e campos **em português**

Detalhes em [docs/02-arquitetura.md](docs/02-arquitetura.md).

---

## 4. Proibições absolutas

| Proibição | Por quê |
|---|---|
| Regra de negócio em Endpoint | Viola Clean Architecture, impossibilita teste |
| Regra de negócio em Repositório | Repositório é persistência, não conhece regra |
| Acessar o `DbContext` de outro módulo | Destrói a modularidade |
| Implementar controle de estoque | Fora de escopo por decisão do responsável |
| String de conexão, senha ou segredo no código-fonte | Fica no histórico do git para sempre |
| Commitar `appsettings.Development.json` ou `.env.local` | Quebra o ambiente de todo mundo |
| SQL por concatenação de string | SQL injection |
| `.Result` ou `.Wait()` em código async | Deadlock |
| `async void` | Exceção silenciada |
| `any` no TypeScript | `strict: true` existe por um motivo |
| `git push` direto em `main` ou `develop` | Todo código entra por PR |
| `git commit --no-verify` | Hook que falha se corrige, não se contorna |
| `git push --force` em branch compartilhada | Apaga trabalho alheio. Use `--force-with-lease`, e só na sua branch |
| Merge de PR sem CI verde ou sem aprovação | É exatamente o que estamos treinando a não fazer |

---

## 5. Regras para IA

**5.1 Ler antes de escrever.** Nenhum arquivo é modificado sem ter sido lido. Sem inferência sobre
código não lido.

**5.2 Não inventar regra de negócio.** O que não está em `docs/01-escopo-funcional.md` não existe.
Se faltar informação, marcar `⚠️ PENDENTE DE VALIDAÇÃO` e perguntar — nunca supor.

**5.3 Sem criatividade arquitetural.** A arquitetura está definida em `docs/02-arquitetura.md`.
Alternativas se propõem ao responsável, não se implementam por conta própria.

**5.4 Sem escopo extra.** Foi pedido o campo X? Entrega o campo X. "Já que eu estava aqui, também
melhorei Y" transforma um PR de 40 linhas em um de 400 e destrói o review.

**5.5 Confirmar antes de destruir.** Reset de banco, exclusão de arquivos, reescrita ampla,
`git push --force`: só com confirmação explícita.

**5.6 Um PR, um assunto.** Se a tarefa não cabe em ~400 linhas alteradas, quebrar em dois PRs e
avisar.

---

## 6. Fluxo de trabalho

| Branch | Origem | Destino | Merge |
|---|---|---|---|
| `feature/{n}-descricao` | `develop` | `develop` | Squash |
| `fix/{n}-descricao` | `develop` | `develop` | Squash |
| `release/{versao}` | `develop` | `main` | Merge commit + tag |
| `hotfix/{n}-descricao` | `main` | `main` (e depois `develop`) | Merge commit + tag |

Commits: `{tipo}({escopo}): {descrição em português}` — tipos `feat`, `fix`, `test`, `docs`,
`refactor`, `chore`, `ci`, `perf`; escopos `cadastros`, `movimentos`, `host`, `frontend`, `ci`,
`docs`, `deps`.

Detalhes, incluindo review, conflitos e release: [docs/03-fluxo-git.md](docs/03-fluxo-git.md).

---

## 7. Definition of Done

- [ ] Faz o que a issue pediu — nem menos, nem mais
- [ ] Regra de negócio no Domain/Application
- [ ] Nenhum módulo acessando o `DbContext` de outro
- [ ] Teste unitário para toda regra de negócio nova
- [ ] `dotnet build` e `dotnet test` verdes
- [ ] `npm run type-check` e `npm run build` verdes
- [ ] Sem segredo no código
- [ ] PR com **o quê / por quê / como testar / decisões**
- [ ] CI verde e 1 aprovação
- [ ] Termo de negócio novo no glossário (`docs/01-escopo-funcional.md` §5)
