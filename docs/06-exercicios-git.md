# 06 — Roteiro de Exercícios de Git

**Status:** proposta para validação
**Versão:** 1.0

Estes exercícios são feitos **pela equipe, depois da v1.0.0**. Estão em ordem crescente de
dificuldade. Cada um vira uma issue no GitHub.

Sugestão de formato: uma sessão por semana, ~1h, com todo mundo na mesma sala (ou chamada) e a tela
compartilhada.

---

## Nível 1 — O ciclo básico

### Exercício 1.1 — Sua primeira feature

**Todos fazem, individualmente.**

Issue: *"Adicionar campo Observação ao cadastro de produto"*

1. Criar issue no GitHub
2. `git switch develop; git pull`
3. `git switch -c feature/{n}-observacao-produto`
4. Backend: campo na entidade, migration, DTO, endpoint
5. Frontend: campo no modal e coluna na tabela
6. Commit no formato correto, push, PR com descrição completa
7. Pedir review de um colega
8. Aplicar o que foi pedido, merge, deletar a branch

**O que se aprende:** o ciclo inteiro, do zero ao merge.

### Exercício 1.2 — Review de verdade

**Em duplas, invertendo os papéis.**

Revise o PR do colega usando o checklist de `03-fluxo-git.md` §5. Obrigatório deixar pelo menos:
um `[bloqueante]` ou `[dúvida]`, e um `[elogio]`.

**O que se aprende:** review não é carimbo. E receber crítica de código é uma habilidade.

---

## Nível 2 — Conflitos e histórico

### Exercício 2.1 — Conflito programado

**Duas pessoas, simultaneamente.**

- Pessoa A: `feature/{n}-cadastro-clientes` — substitui o texto livre "Cliente" da venda por um
  cadastro real
- Pessoa B: `feature/{m}-cadastro-fornecedores` — mesma coisa para "Fornecedor" da compra

As duas mexem no `Roteador.tsx`, no menu lateral e no `.slnx`. A primeira que fizer merge ganha; a
segunda **vai ter conflito** e precisa resolver.

1. A segunda pessoa: `git fetch origin; git rebase origin/develop`
2. Resolver cada conflito, entendendo o que cada lado queria
3. `git rebase --continue`, `git push --force-with-lease`
4. Conferir que o PR agora mostra só as mudanças dela

**O que se aprende:** conflito é normal, e resolver exige entender o código do outro. Também
mostra na prática por que branch curta importa.

### Exercício 2.2 — Desfazer sem destruir

Issue: *"Reverter o cadastro de fornecedores — decidimos manter texto livre"*

- `git revert -m 1 <sha-do-merge>` na `develop`, via PR
- Comparar com o que aconteceria com `git reset --hard` (fazer numa branch descartável para ver)

**O que se aprende:** `revert` cria história, `reset` apaga história. Em branch compartilhada só o
primeiro é aceitável.

### Exercício 2.3 — Achar o culpado

Um bug foi introduzido em algum ponto entre a `v1.0.0` e a `develop` atual.

- `git log --oneline --graph`, `git blame`, `git show`
- `git bisect start` / `git bisect bad` / `git bisect good v1.0.0`

**O que se aprende:** o histórico só serve para investigar se os commits forem pequenos e bem
descritos. É aqui que a regra de commit boa se paga.

---

## Nível 3 — Release e produção

### Exercício 3.1 — Hotfix

**Bug plantado propositalmente na v1.0.0**: em vendas com mais de 3 itens, o valor total ignora o
último item.

1. Reproduzir (e escrever o teste que falha **antes** de corrigir)
2. `git switch main; git pull; git switch -c hotfix/{n}-total-ignora-ultimo-item`
3. Corrigir, PR para `main`, merge commit
4. Tag `v1.0.1`
5. **Trazer para `develop`** — a etapa que todo mundo esquece

**O que se aprende:** por que hotfix sai de `main` e não de `develop`, e por que precisa voltar.

### Exercício 3.2 — Release 1.1.0

Acumular 2 ou 3 features em `develop`, então:

1. `release/1.1.0`, `CHANGELOG.md` escrito a partir do histórico de commits
2. PR para `main` com merge commit
3. Tag `v1.1.0`, merge de volta em `develop`

**O que se aprende:** o CHANGELOG só é fácil de escrever se os commits forem bons. Fecha o ciclo
que começou no Nível 1.

### Exercício 3.3 — Cherry-pick

Uma correção feita em `develop` precisa ir para `main` **sem** levar junto as features que ainda
não foram liberadas.

- `git switch -c hotfix/{n}-... main`
- `git cherry-pick <sha>`

**O que se aprende:** commit atômico (uma mudança por commit) é o que torna cherry-pick possível.

---

## Nível 4 — Refino

### Exercício 4.1 — Limpar a branch antes do PR

Trabalhe com commits sujos de propósito (`wip`, `ajuste`, `agora vai`) e antes do PR:

```bash
git rebase -i origin/develop
```

Use `squash`, `reword`, `drop` e `reorder` para transformar 8 commits ruins em 2 bons.

**O que se aprende:** o histórico é um produto — ele se edita antes de publicar.

### Exercício 4.2 — Trabalhar em duas coisas ao mesmo tempo

Estando no meio de uma feature, chega um pedido urgente:

- Opção A: `git stash` → resolve → `git stash pop`
- Opção B: `git worktree add ../urgente main` → duas pastas, duas branches, ao mesmo tempo

**O que se aprende:** você nunca precisa "terminar antes de trocar de assunto".

### Exercício 4.3 — Recuperar o que "sumiu"

Faça, numa branch descartável: `git reset --hard HEAD~3`. Depois recupere tudo com `git reflog`.

**O que se aprende:** o git quase nunca perde nada de verdade. Isso reduz o medo de experimentar —
que é o principal bloqueio de quem está aprendendo.

---

## Acompanhamento

| Pessoa | 1.1 | 1.2 | 2.1 | 2.2 | 2.3 | 3.1 | 3.2 | 3.3 | 4.1 | 4.2 | 4.3 |
|---|---|---|---|---|---|---|---|---|---|---|---|
| | | | | | | | | | | | |
| | | | | | | | | | | | |
| | | | | | | | | | | | |
