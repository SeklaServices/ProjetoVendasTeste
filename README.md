# ProjetoVendasTeste

Sistema web simples de vendas — cadastro de produtos, entradas de compra e saídas de venda.

**O sistema é o pretexto.** Este repositório existe para a equipe praticar, com um projeto real e
pequeno, o fluxo de trabalho que queremos usar no projeto oficial: issues, branches, pull requests,
code review, resolução de conflitos, releases e hotfixes.

> Se você chegou aqui para entender **como trabalhamos**, leia a seção
> [Como funciona o fluxo de trabalho](#como-funciona-o-fluxo-de-trabalho). É a parte que importa.

---

## Índice

- [O que o sistema faz](#o-que-o-sistema-faz)
- [Stack](#stack)
- [Como rodar](#como-rodar)
- [**Como funciona o fluxo de trabalho**](#como-funciona-o-fluxo-de-trabalho)
  - [O ciclo em uma frase](#o-ciclo-em-uma-frase)
  - [1. Issue](#1-issue--toda-tarefa-começa-aqui)
  - [2. Branch](#2-branch--uma-issue-uma-branch)
  - [3. Commits](#3-commits--pequenos-e-descritivos)
  - [4. Pull Request](#4-pull-request--onde-o-trabalho-é-conferido)
  - [5. Code review](#5-code-review--todo-mundo-revisa-todo-mundo)
  - [6. Merge](#6-merge--e-a-branch-morre)
  - [7. Release e hotfix](#7-release-e-hotfix)
  - [Conflitos](#conflitos)
  - [Por que fazer assim](#por-que-fazer-assim)
- [Documentação completa](#documentação-completa)

---

## O que o sistema faz

| Módulo | O que faz |
|---|---|
| **Produtos** | Cadastro com código único, nome, unidade, preço de custo e de venda |
| **Compras** | Entrada de mercadoria — cabeçalho (data, fornecedor) + itens (produto, quantidade, preço) |
| **Vendas** | Saída de mercadoria — mesma estrutura, com cliente no lugar de fornecedor |
| **Resumo** | Totais de compras e vendas no período, e as últimas movimentações |

**O que ele deliberadamente NÃO faz:** controle de estoque, cadastro de clientes/fornecedores,
login, fiscal, financeiro. Ver [docs/01-escopo-funcional.md](docs/01-escopo-funcional.md) §3.

---

## Stack

| Camada | Tecnologia |
|---|---|
| Backend | .NET 10 — ASP.NET Core Minimal API, Clean Architecture, EF Core 10 |
| Banco | SQL Server local — **um por desenvolvedor** ([por quê](docs/07-decisoes.md#d-001--cada-desenvolvedor-tem-seu-próprio-banco-de-dados-local)) |
| Frontend | React 19 + TypeScript strict + Vite + Ant Design 6 + TanStack Query + Axios |
| CI | GitHub Actions |

É a mesma stack do projeto oficial, em escala reduzida.

---

## Como rodar

Passo a passo completo, incluindo pré-requisitos e problemas comuns:
**[docs/05-setup.md](docs/05-setup.md)**.

Resumo:

```bash
git clone https://github.com/SeklaServices/ProjetoVendasTeste.git
cd ProjetoVendasTeste
```

```bash
cd backend && dotnet run --project src/Host/Vendas.Host
```

```bash
cd frontend && npm install && npm run dev
```

---

# Como funciona o fluxo de trabalho

Esta seção descreve como o trabalho anda neste repositório. É o mesmo fluxo que queremos no
projeto oficial.

## O ciclo em uma frase

**Toda mudança no sistema nasce de uma issue, é feita numa branch própria, entra por pull request,
é revisada por outra pessoa e só então vira parte do produto.**

Nada entra por push direto. Nem uma linha, nem do autor do projeto.

```
issue  →  branch  →  commits  →  pull request  →  review  →  merge  →  branch deletada
```

## As branches permanentes

| Branch | O que é | Quem escreve nela |
|---|---|---|
| `main` | O que está "em produção". Cada versão publicada tem uma tag (`v1.0.0`, `v1.0.1`…). | Ninguém diretamente. Só recebe merge de `release/*` ou `hotfix/*`. |
| `develop` | Onde o trabalho de todos se integra. É a base de toda feature. | Ninguém diretamente. Só recebe merge de PR. |

Todas as outras branches são temporárias: nascem, entregam, morrem.

```
main      ──●────────────────────────────●───────────●──   tags: v1.0.0   v1.0.1
             \                          /           /
release       \                    ●───●           /
               \                  /               /
develop   ──●───●────●────●──────●───────────────/──────
             \    \    \  /                     /
feature       ●──●  ●──●                  hotfix ●
```

## 1. Issue — toda tarefa começa aqui

Antes de escrever código, existe uma **issue** no GitHub descrevendo o que precisa ser feito.
Existem três templates, e o GitHub não deixa abrir issue em branco:

| Template | Quando usar |
|---|---|
| **Funcionalidade** | Algo novo que o sistema passa a fazer |
| **Bug** | Algo que o sistema faz errado |
| **Tarefa técnica** | Refatoração, dependência, CI, documentação |

A issue precisa ter **critérios de aceite** — a lista verificável que o revisor vai usar depois
para dizer "está pronto". Issue sem critério de aceite gera PR que ninguém sabe se aprova.

> **Por que issue antes de código:** a issue é onde a discussão sobre *o que fazer* acontece.
> Se ela acontecer só no PR, alguém já gastou dois dias implementando a coisa errada.

## 2. Branch — uma issue, uma branch

O número da branch é o número da issue. É isso que liga tudo: issue → branch → commits → PR → merge.

| Padrão | Sai de | Volta para | Quando |
|---|---|---|---|
| `feature/{n}-descricao` | `develop` | `develop` | Funcionalidade nova |
| `fix/{n}-descricao` | `develop` | `develop` | Bug encontrado antes de publicar |
| `release/{versao}` | `develop` | `main` | Publicar uma versão |
| `hotfix/{n}-descricao` | `main` | `main` **e** `develop` | Bug urgente no que já está publicado |

```bash
git switch develop
git pull origin develop                      # sempre partir da develop ATUALIZADA
git switch -c feature/12-cadastro-produtos
```

**Branch curta é a regra.** Uma branch que vive duas semanas é conflito garantido — o resto do
time andou, e o seu código foi escrito contra um passado que não existe mais.

## 3. Commits — pequenos e descritivos

Formato obrigatório:

```
{tipo}({escopo}): {descrição em português, no imperativo, sem ponto final}
```

**Tipos:** `feat` `fix` `test` `docs` `refactor` `chore` `ci` `perf`
**Escopos:** `cadastros` `movimentos` `host` `frontend` `ci` `docs` `deps`

```
feat(cadastros): adiciona validacao de codigo duplicado no produto
fix(movimentos): corrige calculo do total quando o item e removido
test(movimentos): cobre rejeicao de compra sem itens
docs(git): documenta o fluxo de hotfix
```

Um bom commit descreve **o que muda no comportamento do sistema**. `ajustes`, `wip` e
`alterado ProdutoService.cs` não descrevem nada.

> **Por que isso importa:** o histórico é a única documentação que nunca fica desatualizada.
> Quando alguém perguntar "por que essa linha existe?", `git blame` só responde se a mensagem
> do commit responder. E o CHANGELOG de uma release se escreve a partir dos commits — se eles
> forem ruins, alguém vai reconstruí-los a mão.

## 4. Pull Request — onde o trabalho é conferido

```bash
git push -u origin feature/12-cadastro-produtos
```

O GitHub oferece o link para abrir o PR. O [template](.github/pull_request_template.md) é
preenchido automaticamente e pede quatro coisas:

| Campo | Para quê |
|---|---|
| **O quê** | O que o PR faz, em 1–3 frases |
| **Por quê** | `Closes #12` — qual issue isso fecha |
| **Como testar** | Passo a passo para o revisor **rodar e validar**. Não é opcional. |
| **Decisões** | O que você escolheu quando havia mais de um caminho, e por quê |

**Tamanho:** alvo de até ~400 linhas alteradas. PR maior que isso não é revisado — é carimbado.
Se não couber, quebre em dois (ex.: backend primeiro, frontend depois).

**Requisitos para o botão de merge liberar:**

- [ ] CI verde (build + testes, backend e frontend)
- [ ] 1 aprovação de outra pessoa
- [ ] Branch atualizada com a base, sem conflito

O GitHub não deixa o autor aprovar o próprio PR. É intencional.

## 5. Code review — todo mundo revisa todo mundo

Revisar código é uma habilidade que se treina, e é onde o conhecimento do sistema se espalha pelo
time. Aqui, quem revisa **roda o PR**, não só lê o diff.

**O que o revisor verifica:**

- Faz o que a issue pediu — nem menos, nem mais
- Regra de negócio no lugar certo (nunca em endpoint nem em repositório)
- Nenhum segredo, senha ou string de conexão no código
- Regra nova tem teste
- Consegui rodar seguindo o "Como testar"

**Como comentar** — sobre o código, nunca sobre a pessoa, e sempre classificado:

| Prefixo | Significado |
|---|---|
| `[bloqueante]` | Precisa mudar antes do merge |
| `[sugestão]` | Melhoraria, mas não bloqueia |
| `[dúvida]` | Não entendi, me explica |
| `[elogio]` | Isso ficou bom — use, review não é só apontar erro |

> `[bloqueante]` Esse `if` de "compra precisa ter item" está no endpoint. Se alguém chamar o
> handler de outro lugar, a regra não roda. Sobe para `Compra.Criar()`?

**Como receber:** todo comentário merece resposta — ou você muda, ou explica por que não. "Feito"
e "resolvi de outro jeito porque X" são as duas respostas válidas. Silêncio não é uma delas.

## 6. Merge — e a branch morre

| De → para | Estratégia | Por quê |
|---|---|---|
| `feature/*`, `fix/*` → `develop` | **Squash and merge** | Os commits da branch viram um só. O histórico da `develop` fica com um commit por funcionalidade — legível. |
| `release/*`, `hotfix/*` → `main` | **Merge commit** | Preserva o histórico e deixa explícito no grafo que houve uma publicação. |

Depois do merge, a branch é deletada (o GitHub faz isso sozinho). Limpando localmente:

```bash
git switch develop
git pull origin develop
git branch -d feature/12-cadastro-produtos
```

## 7. Release e hotfix

**Release** — quando a `develop` acumula um conjunto que faz sentido publicar:

```bash
git switch -c release/1.0.0          # a partir de develop
# CHANGELOG, versão — nada de funcionalidade nova aqui
# PR para main → merge commit
git tag -a v1.0.0 -m "Release 1.0.0"
git push origin v1.0.0
git switch develop && git merge main && git push    # devolver os ajustes
```

**Hotfix** — bug em produção que não pode esperar:

```bash
git switch main && git pull
git switch -c hotfix/31-total-negativo
# corrigir + escrever o teste que reproduz o bug
# PR para main → merge commit → tag v1.0.1
git switch develop && git merge main && git push    # ← a etapa que todo mundo esquece
```

Hotfix sai de `main`, não de `develop` — senão a correção viria junto com funcionalidades ainda
não publicadas. E **precisa** voltar para `develop`, senão o bug reaparece na próxima release.

## Conflitos

Conflito não é erro. É o git avisando que duas pessoas mudaram a mesma coisa e ele não vai chutar
qual das duas está certa.

```bash
git fetch origin
git rebase origin/develop
# CONFLICT (content): Merge conflict in .../ProdutosEndpoints.cs

git status                       # quais arquivos conflitam
# abrir cada um, decidir o conteúdo final, apagar <<<<<<< ======= >>>>>>>
git add <arquivo>
git rebase --continue

git push --force-with-lease      # nunca --force puro

# deu ruim? volta tudo:
git rebase --abort
```

`--force-with-lease` recusa o push se outra pessoa tiver mexido na sua branch. `--force` puro
apaga o trabalho dela sem avisar. **Nunca force push em `main` ou `develop`.**

## Por que fazer assim

| Regra | O que ela evita |
|---|---|
| Nada de push direto em `main`/`develop` | Alguém quebra a base de todo mundo às 18h de sexta |
| Toda mudança passa por PR | Ninguém é a única pessoa que sabe como uma parte funciona |
| CI obrigatório | "Na minha máquina funciona" deixa de ser um argumento |
| 1 aprovação obrigatória | Erro óbvio é pego antes de virar bug em produção |
| Branch curta | Conflitos pequenos em vez de gigantes |
| Commit descritivo | `git blame` e `git bisect` viram ferramentas úteis em vez de decorativas |
| Issue antes do código | A discussão sobre *o que fazer* acontece antes de dois dias de trabalho |

Nenhuma dessas regras existe por burocracia. Cada uma é a resposta a um problema que já aconteceu
em algum projeto.

---

## Documentação completa

| # | Documento | O que responde |
|---|---|---|
| 0 | [CLAUDE.md](CLAUDE.md) | Regras do projeto — obrigatório para IA e humanos |
| 1 | [docs/01-escopo-funcional.md](docs/01-escopo-funcional.md) | O que o sistema faz e o que não faz |
| 2 | [docs/02-arquitetura.md](docs/02-arquitetura.md) | Camadas, pastas, modelo de dados, contratos de API |
| 3 | [docs/03-fluxo-git.md](docs/03-fluxo-git.md) | O fluxo em detalhe, com todos os comandos |
| 4 | [docs/04-plano-de-desenvolvimento.md](docs/04-plano-de-desenvolvimento.md) | Os PRs que compõem a v1.0.0 |
| 5 | [docs/05-setup.md](docs/05-setup.md) | Montar o ambiente do zero |
| 6 | [docs/06-exercicios-git.md](docs/06-exercicios-git.md) | 11 exercícios práticos, em 4 níveis |
| 7 | [docs/07-decisoes.md](docs/07-decisoes.md) | Decisões tomadas e o porquê de cada uma |
