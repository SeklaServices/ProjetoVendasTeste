# 09 — O mesmo fluxo, sem digitar comandos

**Para quem:** quem está começando e prefere botões a linha de comando.

Este documento faz **exatamente** o que `03-fluxo-git.md` descreve — mesmas branches, mesmos PRs,
mesmas regras. Só troca os comandos por cliques. Nada aqui é "o jeito fácil e errado": é o mesmo
fluxo, por outra porta.

Ferramentas: **navegador** (github.com) + **VS Code** (painel Source Control).

---

## 1. O que dá para fazer 100% pela web

| Tarefa | Onde |
|---|---|
| Criar issue | Aba **Issues → New issue** |
| Criar branch a partir da issue | Dentro da issue, painel direito, **Create a branch** |
| Abrir Pull Request | Aba **Pull requests → New pull request** |
| Revisar código, comentar, aprovar | Aba **Files changed** do PR |
| Resolver conflito simples | Botão **Resolve conflicts** dentro do PR |
| Fazer merge e apagar a branch | Botões no rodapé do PR |
| Criar tag e release | Aba **Releases → Draft a new release** |
| Editar um arquivo pequeno (documentação) | Ícone de lápis no arquivo → **Commit changes** |

**O que não dá pela web:** escrever código de verdade, rodar o sistema e rodar os testes. Para
isso, a seção 3.

---

## 2. Preparar o VS Code (uma vez só)

1. Abrir a pasta do projeto: **File → Open Folder → `C:\ProjetosClaude\ProjetoTesteVendas`**
2. Instalar a extensão **GitHub Pull Requests** (ícone de blocos na barra esquerda, buscar pelo
   nome). Ela traz os PRs e as issues para dentro do editor.
3. Fazer login: a extensão pede na primeira vez, pelo navegador.

A partir daí, o ícone de **Source Control** (o terceiro na barra esquerda, parece uma bifurcação)
é o seu painel de git.

---

## 3. O ciclo completo de uma issue, por botões

### Passo 1 — Criar a branch (pela web)

1. Abrir a issue no GitHub
2. No painel da direita, em **Development**, clicar em **Create a branch**
3. O GitHub sugere um nome. **Troque** para o nosso padrão: `feature/2-contas-a-receber`
4. Em **Branch source**, escolher **`develop`** — não deixe `main`. Este é o ponto onde mais se
   erra pela web
5. Clicar em **Create branch**

Isso cria a branch já ligada à issue: quando o PR for mergeado, a issue fecha sozinha.

### Passo 2 — Trazer a branch para a sua máquina (VS Code)

1. Na **barra inferior azul** do VS Code, no canto esquerdo, aparece o nome da branch atual
2. Clicar nele → abre a lista de branches
3. Clicar no ícone de **atualizar** (🔄) se a nova branch não aparecer, ou escolher
   **origin/feature/2-contas-a-receber**

Pronto: o VS Code baixou e trocou para a branch. Equivale a `git fetch` + `git switch`.

### Passo 3 — Escrever o código

Normal, no editor. Salve os arquivos.

Para conferir que funciona, dois comandos no terminal — só esses dois, e são sempre iguais:

```powershell
cd backend; dotnet build; dotnet test
```

```powershell
cd frontend; npm run type-check
```

Se algum falhar, corrija antes de continuar. Falhar aqui é bom: falhar no PR custa um ciclo inteiro.

### Passo 4 — Commit (VS Code, por botões)

1. Abrir o painel **Source Control** (ícone da bifurcação)
2. Os arquivos alterados aparecem em **Changes**
3. Passar o mouse em cada arquivo e clicar no **`+`** para incluí-lo no commit
   (ou no `+` do cabeçalho **Changes** para incluir todos) — isso é o `git add`
4. Escrever a mensagem na caixa de texto, **no formato obrigatório**:
   ```
   feat(frontend): adiciona tela de contas a receber
   ```
5. Clicar em **Commit**

> **Commits pequenos.** Não junte tudo num commit só no fim. Terminou uma parte que faz sentido
> sozinha? Commit. É isso que faz `git blame` e o CHANGELOG funcionarem depois.

### Passo 5 — Enviar (push)

No mesmo painel, clicar em **Sync Changes** (ou na setinha ↑ da barra azul de baixo).

### Passo 6 — Abrir o PR (pela web)

1. Ir ao repositório no GitHub — aparece uma faixa amarela **"Compare & pull request"**. Clicar
2. **Conferir o `base`**: tem que estar **`develop`**, não `main`. O GitHub às vezes sugere `main`
3. O template já vem preenchido. Preencher de verdade:
   - **O quê** — o que o PR faz
   - **Por quê** — `Closes #2`
   - **Como testar** — passo a passo para o revisor rodar
   - **Decisões** — o que você escolheu quando havia mais de um caminho
4. À direita, em **Reviewers**, escolher a pessoa
5. **Create pull request**

### Passo 7 — Esperar

Duas coisas precisam ficar verdes no rodapé do PR:

- **All checks have passed** — o CI (build e testes)
- **1 approval** — a revisão

Se o CI ficar vermelho, clicar em **Details** para ver o erro, corrigir na sua máquina, commit e
Sync. O PR atualiza sozinho.

### Passo 8 — Merge

1. Botão **Squash and merge** (é o único que a `develop` aceita)
2. **Confirm squash and merge**
3. **Delete branch** — o botão aparece logo depois

### Passo 9 — Voltar para a develop atualizada

No VS Code: clicar no nome da branch na barra azul → escolher **develop** → clicar em
**Sync Changes**.

**Nunca comece a próxima tarefa sem fazer isso.** É o erro mais comum de quem começa: criar a
branch nova a partir de uma `develop` velha.

---

## 4. Revisar o PR de outra pessoa (100% web)

1. Abrir o PR → aba **Files changed**
2. Ler o diff. Verde é o que entrou, vermelho é o que saiu
3. Para comentar numa linha: passar o mouse sobre ela e clicar no **`+`** azul que aparece à
   esquerda
4. Escrever o comentário **classificado**:
   - `[bloqueante]` precisa mudar antes do merge
   - `[sugestão]` melhoraria, mas não bloqueia
   - `[dúvida]` não entendi, me explica
   - `[elogio]` isso ficou bom
5. **Start a review** (não "Add single comment" — assim os comentários saem todos juntos)
6. Ao terminar, botão verde **Review changes**, no topo direito:
   - **Comment** — observações, sem julgamento
   - **Approve** — pode entrar
   - **Request changes** — tem `[bloqueante]`, precisa mudar
7. **Submit review**

**Antes de aprovar, rode o PR.** No VS Code, troque para a branch do PR (mesma barra azul) e siga
o "Como testar" que o autor escreveu. Aprovar sem rodar é carimbar.

---

## 5. Resolver conflito (pela web, quando é simples)

Quando o PR mostrar **"This branch has conflicts that must be resolved"**:

1. Clicar em **Resolve conflicts**
2. Abre um editor com o arquivo. Os marcadores são:
   ```
   <<<<<<< feature/3-contas-a-pagar
   { key: '/contas-a-pagar', label: <Link to="/contas-a-pagar">Contas a pagar</Link> },
   =======
   { key: '/contas-a-receber', label: <Link to="/contas-a-receber">Contas a receber</Link> },
   >>>>>>> develop
   ```
3. Decidir o conteúdo final. **Neste caso as duas linhas devem ficar** — uma feature adicionou o
   menu de pagar, a outra o de receber, e o sistema precisa dos dois
4. Apagar as três linhas de marcação (`<<<<<<<`, `=======`, `>>>>>>>`)
5. **Mark as resolved** → **Commit merge**

> Conflito não é erro. É o git avisando que duas pessoas mudaram a mesma coisa e ele não vai
> chutar qual está certa. Quem decide é você — e por isso precisa entender o que cada lado queria.

Conflito complicado (muitos arquivos, lógica embolada) é melhor resolver no VS Code, que mostra as
duas versões lado a lado com botões **Accept Current** / **Accept Incoming** / **Accept Both**.

---

## 6. De comando para botão

Guarde para quando quiser entender o que está acontecendo por baixo:

| Comando | Onde clicar |
|---|---|
| `git switch develop` | Nome da branch na barra azul do VS Code → escolher |
| `git pull` | **Sync Changes** no painel Source Control |
| `git switch -c feature/...` | **Create a branch** dentro da issue, no GitHub |
| `git add` | `+` no arquivo, no painel Source Control |
| `git commit -m "..."` | Caixa de mensagem + botão **Commit** |
| `git push` | **Sync Changes** |
| `gh pr create` | Faixa **Compare & pull request** no GitHub |
| `git rebase origin/develop` | **Resolve conflicts** no PR, ou **Sync** no VS Code |
| `git branch -d` | **Delete branch** depois do merge |

---

## 7. Os três erros mais comuns de quem começa pela web

1. **Criar a branch a partir de `main`.** A tela de criar branch sugere a branch padrão. Toda
   feature sai de **`develop`**.
2. **Abrir o PR com base `main`.** Mesma armadilha, na outra ponta. Confira o `base` antes de
   criar.
3. **Começar a tarefa nova sem atualizar a `develop`.** Passo 9 existe para isso. Pular esse passo
   é o que transforma um conflito pequeno num grande.
