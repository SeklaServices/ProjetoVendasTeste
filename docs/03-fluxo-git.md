# 03 — Fluxo de Trabalho com Git e GitHub

**Status:** proposta para validação
**Versão:** 1.0

> Este é o documento mais importante do projeto. O sistema de vendas existe para dar matéria-prima
> a este fluxo.

---

## 1. Estrutura de branches

Mesmo modelo do `CeasaSystemNext`.

| Branch | Propósito | Vida | Proteção |
|---|---|---|---|
| `main` | "Produção". Só recebe merge de `release/*` ou `hotfix/*`. Cada merge ganha uma tag. | Permanente | CI verde + 1 aprovação. **Sem push direto.** |
| `develop` | Integração. É de onde toda feature nasce e para onde ela volta. | Permanente | CI verde + 1 aprovação. **Sem push direto.** |
| `feature/{n}-descricao` | Uma feature ou tarefa | Morre no merge | — |
| `fix/{n}-descricao` | Correção de bug encontrado em `develop` | Morre no merge | — |
| `release/{versao}` | Preparação de release (`develop` → `main`) | Morre no merge | — |
| `hotfix/{n}-descricao` | Correção urgente direto de `main` | Morre no merge | — |

```
main      ──●────────────────────────────●───────────●──  tags v1.0.0, v1.0.1
             \                          /           /
release       \                    ●───●           /
               \                  /               /
develop   ──●───●────●────●──────●───────────────/──────
             \    \    \  /                     /
feature       ●──●  ●──●                  hotfix ●
```

**`{n}` é o número da issue no GitHub.** Toda branch nasce de uma issue — isso é o que dá
rastreabilidade: da issue → branch → commits → PR → merge → tag.

### Regras absolutas

1. **Nunca** `git push` direto em `main` ou `develop`. (Branch protection vai bloquear de qualquer
   forma — mas a regra existe antes da ferramenta.)
2. Feature nasce de `develop` **atualizada**: `git switch develop; git pull` antes de criar.
3. Uma branch = uma tarefa. Branch que faz duas coisas gera PR que ninguém revisa direito.
4. Branch de feature vive **poucos dias**. Branch de duas semanas é conflito garantido.
5. `--force` só em branch de feature **sua**, nunca em branch compartilhada.
6. `--no-verify` é proibido. Hook que falha se corrige, não se contorna.

---

## 2. Ciclo completo de uma tarefa

```bash
# 1. Partir de develop atualizada
git switch develop
git pull origin develop

# 2. Criar a branch (n = número da issue)
git switch -c feature/12-cadastro-produtos

# 3. Trabalhar, commitando em passos pequenos
git add .
git commit -m "feat(cadastros): adiciona entidade Produto e regras de validacao"

# 4. Publicar a branch
git push -u origin feature/12-cadastro-produtos

# 5. Abrir o PR (pela interface do GitHub ou pelo gh CLI)
gh pr create --base develop --fill

# 6. Esperar CI verde + review. Ajustar o que for pedido, commitar, push.

# 7. Merge pelo GitHub (Squash and merge). Depois, limpar:
git switch develop
git pull origin develop
git branch -d feature/12-cadastro-produtos
```

### Quando `develop` andou enquanto você trabalhava

```bash
git switch feature/12-cadastro-produtos
git fetch origin
git rebase origin/develop        # replica seus commits em cima da develop nova
# resolver conflitos, se houver:
#   git add <arquivo>
#   git rebase --continue
git push --force-with-lease      # --force-with-lease, nunca --force puro
```

**Por que `rebase` e não `merge`?** Porque o histórico da branch de feature fica linear e o PR mostra
só o que você fez, sem commits de merge no meio. E porque `--force-with-lease` recusa o push se
outra pessoa tiver mexido na sua branch — protege contra sobrescrever trabalho alheio.

---

## 3. Commits

Formato obrigatório — igual ao projeto real:

```
{tipo}({escopo}): {descrição em português, imperativo, minúscula, sem ponto final}
```

**Tipos aceitos:** `feat`, `fix`, `test`, `docs`, `refactor`, `chore`, `ci`, `perf`

**Escopos deste projeto:** `cadastros`, `movimentos`, `estoque`, `host`, `frontend`, `ci`, `docs`, `deps`

```
feat(cadastros): adiciona validacao de codigo duplicado no produto
fix(movimentos): corrige calculo do valor total quando o item e removido
test(movimentos): cobre rejeicao de compra sem itens
refactor(frontend): extrai grid de itens para componente reutilizavel
docs(git): documenta o fluxo de hotfix
chore(deps): atualiza ant design para 6.1
ci: separa job de frontend do job de backend
```

**O que faz um commit ser bom aqui:** ele descreve **o que muda no comportamento do sistema**, não o
que você digitou. `feat(cadastros): adiciona validacao de codigo duplicado` é útil.
`ajustes`, `wip`, `correções`, `alterado ProdutoService.cs` não são.

---

## 4. Pull Requests

### Todo PR precisa ter

1. **Título no formato de commit**: `feat(cadastros): cadastro de produtos`
2. **Descrição preenchida** conforme `.github/pull_request_template.md`:
   - **O quê** — o que foi feito
   - **Por quê** — qual issue resolve (`Closes #12`)
   - **Como testar** — passo a passo para o revisor validar rodando
   - **Decisões** — o que você escolheu e por quê, quando não era óbvio
3. **CI verde**
4. **1 aprovação** de outra pessoa
5. Estar **atualizado com a base** (sem conflito)

### Tamanho

Alvo: **até ~400 linhas alteradas**. PR maior que isso não é revisado — é carimbado. Se sua tarefa
não cabe, quebre em dois PRs (ex.: backend primeiro, frontend depois — foi assim que o plano em
`04-plano-de-desenvolvimento.md` foi montado).

### Estratégia de merge

**Squash and merge** para `feature/*` e `fix/*` → `develop`.
Os commits da branch viram um só, com a mensagem do título do PR. Histórico de `develop` fica
legível: um commit por feature.

**Merge commit** para `release/*` e `hotfix/*` → `main`.
Preserva o histórico e deixa explícito no grafo que houve uma release.

**Sempre deletar a branch** após o merge (o GitHub oferece o botão).

---

## 5. Code review

Revisar é uma habilidade que se treina. Neste projeto **todo mundo revisa todo mundo**.

### O que o revisor verifica

**Funcional**
- [ ] Faz o que a issue pediu — nem menos, nem mais
- [ ] Consegui rodar seguindo o "Como testar" do PR

**Arquitetura**
- [ ] Nenhuma regra de negócio em endpoint ou repositório
- [ ] Nenhum módulo acessando o `DbContext` do outro
- [ ] Camada certa para cada coisa (Domain / Application / Infrastructure / Api)

**Código**
- [ ] Nomes em português, coerentes com o glossário
- [ ] Sem `any` no TypeScript, sem `.Result`/`.Wait()` no C#
- [ ] Sem segredo, string de conexão ou senha no código
- [ ] Sem código comentado e sem `console.log` esquecido

**Testes**
- [ ] Regra de negócio nova tem teste
- [ ] `dotnet test` verde

### Como escrever um comentário de review

Comente sobre o código, nunca sobre a pessoa. Classifique o comentário:

| Prefixo | Significado |
|---|---|
| `[bloqueante]` | Precisa mudar antes do merge |
| `[sugestão]` | Melhoraria, mas não bloqueia |
| `[dúvida]` | Não entendi — me explica |
| `[elogio]` | Isso ficou bom (sim, use — review não é só apontar erro) |

Exemplo:
> `[bloqueante]` Esse `if` de "compra precisa ter item" está no endpoint. Se alguém chamar o
> handler de outro lugar, a regra não roda. Sobe para `Compra.Criar()`?

### Como receber um review

Todo comentário merece resposta: ou você muda, ou você explica por que não. "Feito" ou "resolvi de
outro jeito porque X" — as duas são respostas válidas. Silêncio não é.

---

## 6. Release

Quando `develop` acumula um conjunto de features que faz sentido publicar:

```bash
git switch develop; git pull
git switch -c release/1.0.0
# ajustes finais de versão, CHANGELOG — nada de feature nova aqui
git push -u origin release/1.0.0
gh pr create --base main --title "release: 1.0.0"
# após merge (merge commit, não squash):
git switch main; git pull
git tag -a v1.0.0 -m "Release 1.0.0 — produtos, compras e vendas"
git push origin v1.0.0
# devolver os ajustes da release para develop:
git switch develop; git merge main; git push
```

Versionamento: **SemVer** — `MAJOR.MINOR.PATCH`.

---

## 7. Hotfix

Bug em `main` que não pode esperar a próxima release:

```bash
git switch main; git pull
git switch -c hotfix/31-total-negativo
# corrigir + teste que reproduz o bug
git push -u origin hotfix/31-total-negativo
gh pr create --base main --fill
# após merge em main: taguear v1.0.1 e trazer para develop
git switch develop; git merge main; git push
```

**A parte que sempre esquecem:** o hotfix precisa voltar para `develop`. Senão o bug volta na
próxima release.

---

## 8. Conflitos de merge

Conflito não é erro — é o git avisando que duas pessoas mudaram a mesma coisa e ele não vai chutar.

```bash
git rebase origin/develop
# CONFLICT (content): Merge conflict in backend/src/.../ProdutosEndpoints.cs

git status                  # ver quais arquivos conflitam
# abrir o arquivo, decidir o conteúdo final, apagar os marcadores <<<<<<< ======= >>>>>>>
git add backend/src/.../ProdutosEndpoints.cs
git rebase --continue

# se der errado e quiser recomeçar:
git rebase --abort
```

**Como evitar a maioria deles:** branch curta, `git pull` na develop com frequência, e não
reformatar arquivo inteiro no mesmo PR em que muda comportamento.

---

## 9. Configuração do repositório no GitHub

A fazer na criação do repositório (checklist de setup):

- [ ] Repositório criado (privado, na organização do escritório)
- [ ] `main` como branch padrão; `develop` criada a partir dela
- [ ] **Branch protection** em `main` e `develop`:
  - [ ] Require a pull request before merging
  - [ ] Require approvals: **1**
  - [ ] Require status checks to pass: `backend`, `frontend`
  - [ ] Require branches to be up to date before merging
  - [ ] Block force pushes
- [ ] "Automatically delete head branches" habilitado
- [ ] Squash merge habilitado; merge commit habilitado; rebase merge desabilitado
- [ ] Template de PR em `.github/pull_request_template.md`
- [ ] Issues habilitadas — toda branch nasce de uma

> Com apenas 1 aprovação obrigatória e poucos desenvolvedores, o GitHub **não** deixa o autor
> aprovar o próprio PR. Isso é intencional e é parte do exercício.

---

## 10. Comandos de sobrevivência

| Situação | Comando |
|---|---|
| "Onde eu estou?" | `git status` e `git log --oneline --graph --all -20` |
| Commitei na branch errada | `git reset --soft HEAD~1`, troca de branch, commita de novo |
| Quero desfazer o último commit (ainda não pushado) | `git reset --soft HEAD~1` (mantém as alterações) |
| Quero desfazer um commit já pushado | `git revert <sha>` — **nunca** `reset` em branch compartilhada |
| Preciso guardar o trabalho pela metade | `git stash` … `git stash pop` |
| Quero um commit específico de outra branch | `git cherry-pick <sha>` |
| Fiz besteira e quero voltar no tempo | `git reflog` — ele lembra de tudo, inclusive do que você "perdeu" |
| Quem mudou esta linha e por quê | `git blame <arquivo>` e depois `git show <sha>` |
| Descobrir qual commit introduziu um bug | `git bisect start` / `bad` / `good` |
