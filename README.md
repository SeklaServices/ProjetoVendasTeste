# ProjetoVendasTeste

Sistema web simples de vendas — cadastro de produtos, entradas de compra e saídas de venda.

**O sistema é o pretexto.** Este repositório existe para a equipe praticar, com um projeto real e
pequeno, o fluxo de trabalho que queremos usar no projeto oficial: issues, branches, pull requests,
code review, resolução de conflitos, releases e hotfixes.

> **Se você nunca trabalhou com este fluxo, leia este README inteiro.** Ele foi escrito para levar
> alguém do zero até fechar um ciclo completo de trabalho sozinho, explicando o que cada coisa
> significa. Deve levar uns 20 minutos.

---

## Índice

**Entender**
- [1. O que é e por que existe](#1-o-que-é-e-por-que-existe)
- [2. O vocabulário](#2-o-vocabulário) — branch, PR, CI, merge, ruleset
- [3. O que o sistema faz](#3-o-que-o-sistema-faz)

**Preparar**
- [4. Instalar o que precisa](#4-instalar-o-que-precisa)
- [5. Baixar o projeto](#5-baixar-o-projeto)
- [6. Configurar o banco de dados](#6-configurar-o-banco-de-dados) ← **onde colar a string de conexão**
- [7. Rodar o sistema](#7-rodar-o-sistema)

**Trabalhar**
- [8. O fluxo de trabalho](#8-o-fluxo-de-trabalho)
- [9. Trabalhando com o Claude Code](#9-trabalhando-com-o-claude-code)
- [10. Um ciclo completo, do começo ao fim](#10-um-ciclo-completo-do-começo-ao-fim)
- [11. Revisar o código de outra pessoa](#11-revisar-o-código-de-outra-pessoa)
- [12. As regras que o GitHub aplica sozinho](#12-as-regras-que-o-github-aplica-sozinho)
- [13. Quando dá problema](#13-quando-dá-problema)

**Consultar**
- [14. Documentação completa](#14-documentação-completa)

---

# 1. O que é e por que existe

Todo projeto de software com mais de uma pessoa precisa responder a uma pergunta: **como duas
pessoas mexem no mesmo código sem uma atrapalhar a outra, e sem que ninguém quebre o que já
funcionava?**

A resposta que o mercado consolidou tem quatro peças:

1. **Cada tarefa é feita em separado** (uma *branch*), sem afetar o que os outros veem
2. **Toda mudança é proposta antes de entrar** (um *pull request*), nunca aplicada direto
3. **Um robô confere** que o código compila e os testes passam (a *CI*)
4. **Outra pessoa lê e aprova** antes de o código virar parte do produto (o *code review*)

Este repositório é onde a equipe pratica isso com um sistema de verdade, pequeno o bastante para
caber na cabeça em 15 minutos. O que der certo aqui vai para o projeto oficial.

**Critério de sucesso:** não é "o sistema funciona". É **toda pessoa conseguir, sozinha, pegar uma
tarefa, fazer, abrir o PR, receber crítica, resolver um conflito e ver o merge acontecer.**

---

# 2. O vocabulário

Se algum destes termos for novo, leia com calma — o resto do documento os usa o tempo todo.

### Repositório

A pasta do projeto, com **todo o histórico de todas as mudanças já feitas**. Existe uma cópia no
GitHub (o *remoto*, que é a referência de todos) e uma cópia na sua máquina (o *local*).

### Commit

Uma alteração salva no histórico, com autor, data e uma mensagem explicando **o que mudou no
comportamento do sistema**. É a menor unidade de trabalho registrada.

Não é "salvar arquivo" — é "registrar uma mudança que faz sentido sozinha".

### Branch

Uma **linha paralela de trabalho**. Você cria uma branch, mexe à vontade nela, e ninguém mais é
afetado — o código dos outros continua como estava. Quando termina, essa linha é reunida de volta
à principal.

Neste projeto existem duas branches permanentes:

| Branch | O que representa |
|---|---|
| **`main`** | O que está "em produção". Só entra código já publicado e testado |
| **`develop`** | Onde o trabalho de todo mundo se integra. É daqui que toda tarefa nasce |

E as temporárias, que nascem, entregam e morrem:

| Padrão | Para quê | Nasce de | Volta para |
|---|---|---|---|
| `feature/{n}-descricao` | Funcionalidade nova | `develop` | `develop` |
| `fix/{n}-descricao` | Corrigir bug ainda não publicado | `develop` | `develop` |
| `release/{versao}` | Publicar uma versão | `develop` | `main` |
| `hotfix/{n}-descricao` | Corrigir bug urgente já publicado | `main` | `main` **e** `develop` |

O `{n}` é o número da issue. É o que liga tudo: issue → branch → commits → PR → merge.

```
main      ──●────────────────────────────●───────────●──   tags: v1.0.0   v1.0.1
             \                          /           /
release       \                    ●───●           /
               \                  /               /
develop   ──●───●────●────●──────●───────────────/──────
             \    \    \  /                     /
feature       ●──●  ●──●                  hotfix ●
```

### Issue

O registro **do que precisa ser feito**, antes de existir código. É onde a discussão sobre *o que
fazer* acontece — se ela só acontecer depois, no PR, alguém já gastou dois dias implementando a
coisa errada.

Toda issue precisa de **critérios de aceite**: a lista verificável que o revisor vai usar para
dizer "está pronto".

### Pull Request (PR)

O **pedido para que a sua branch entre na `develop`** (ou na `main`). É onde o código é mostrado,
discutido e aprovado. Enquanto o PR está aberto, nada mudou para ninguém — é uma proposta.

O PR mostra o **diff**: linhas verdes são o que entrou, vermelhas o que saiu.

### CI (Integração Contínua)

Um robô do GitHub que, a cada PR, baixa o código numa máquina limpa e confere se ele compila e se
os testes passam. Se ficar vermelho, o merge é bloqueado.

É o que faz "na minha máquina funciona" deixar de ser um argumento.

### Code review

Outra pessoa lê o seu código e diz se pode entrar. Serve para pegar erro antes de virar bug, e
para o conhecimento do sistema não ficar só na cabeça de quem escreveu.

### Merge

O momento em que a sua branch **vira parte** da `develop`. Depois disso, o código é de todos.

### Conflito

Quando duas pessoas mudaram **a mesma parte do mesmo arquivo**, o git não escolhe por conta
própria — ele para e pergunta. **Não é erro**: é o git sendo honesto sobre o que não sabe decidir.

### Ruleset / branch protection

A configuração do GitHub que **impede no servidor** o que as regras dizem. Sem ela, "não faça push
direto na main" é um combinado. Com ela, o push é recusado.

### Tag e release

Uma marca no histórico dizendo "esta é a versão 1.0.0". Serve para saber exatamente qual código
está publicado, e para conseguir voltar a ele.

---

# 3. O que o sistema faz

| Módulo | O que faz |
|---|---|
| **Produtos** | Cadastro com código único, nome, unidade, preço de custo e de venda |
| **Clientes** | Cadastro com código sequencial, nome, CPF/CNPJ, telefone e e-mail |
| **Compras** | Entrada de mercadoria — cabeçalho (data, fornecedor) + itens (produto, quantidade, preço) |
| **Vendas** | Saída de mercadoria — mesma estrutura, com cliente selecionado do cadastro |
| **Resumo** | Totais de compras e vendas no período, e as últimas movimentações |

**O que ele deliberadamente NÃO faz:** controle de estoque, cadastro de fornecedores (a compra usa
texto livre), login, fiscal, financeiro, edição de documento já salvo.

Isso não é esquecimento — é decisão registrada em [docs/07-decisoes.md](docs/07-decisoes.md).
**Vender 100 unidades de um produto que nunca foi comprado é válido neste sistema**, porque não
existe estoque. Se um dia mudar, a decisão antiga é *superada*, não apagada.

### Stack

| Camada | Tecnologia |
|---|---|
| Backend | .NET 10 — ASP.NET Core Minimal API, Clean Architecture, EF Core 10 |
| Banco | SQL Server local — **um por desenvolvedor** ([por quê](docs/07-decisoes.md#d-001--cada-desenvolvedor-tem-seu-próprio-banco-de-dados-local)) |
| Frontend | React 19 + TypeScript strict + Vite + Ant Design 6 + TanStack Query + Axios |
| CI | GitHub Actions |

É a mesma stack do projeto oficial, em escala reduzida.

---

# 4. Instalar o que precisa

| Ferramenta | Versão | Conferir com | Para quê |
|---|---|---|---|
| **.NET SDK** | 10.x | `dotnet --version` | Compilar e rodar o backend |
| **Node.js** | 24.x | `node --version` | Compilar e rodar o frontend |
| **Git** | 2.4x+ | `git --version` | Controle de versão |
| **SQL Server** | Express, Developer ou LocalDB | ver §6 | Banco de dados |
| **VS Code** | atual | — | Editor e revisão de código |
| **GitHub CLI** (`gh`) | 2.x | `gh --version` | Abrir PR pelo terminal (opcional) |

Instalação rápida no Windows (PowerShell como administrador):

```powershell
winget install Microsoft.DotNet.SDK.10
```

```powershell
winget install OpenJS.NodeJS.LTS
```

```powershell
winget install Microsoft.SQLServer.2022.Express
```

```powershell
winget install GitHub.cli
```

Depois de instalar, **feche e reabra o terminal** — senão os comandos não são encontrados.

### Configurar sua identidade no git (uma vez por máquina)

Sem isto, seus commits saem sem autor identificado:

```powershell
git config --global user.name "Seu Nome"
```

```powershell
git config --global user.email "voce@sekla.com.br"
```

```powershell
git config --global pull.rebase true
```

O `pull.rebase true` faz o histórico ficar linear em vez de encher de commits de merge
automáticos.

### Autenticar no GitHub

```powershell
gh auth login
```

Escolha `GitHub.com` → `HTTPS` → `Login with a web browser`, e cole o código que aparecer.

### Extensão do VS Code

Instale **GitHub Pull Requests** (ícone de blocos na barra esquerda, buscar pelo nome). Ela traz
os PRs e as issues para dentro do editor — você revisa e comenta sem abrir o navegador.

---

# 5. Baixar o projeto

```powershell
git clone https://github.com/SeklaServices/ProjetoVendasTeste.git
```

```powershell
cd ProjetoVendasTeste
```

Abra a pasta no VS Code: **File → Open Folder**.

> **Windows PowerShell 5.1 não aceita `&&`** para encadear comandos — dá erro de sintaxe. Use `;`
> (funciona em PowerShell, `pwsh` e Git Bash). Toda a documentação usa `;`.

---

# 6. Configurar o banco de dados

**Este é o passo que mais trava quem está começando.** Leia com atenção.

Cada desenvolvedor tem o **seu próprio banco, na própria máquina**. Nada é compartilhado. O motivo
está em [docs/07-decisoes.md D-001](docs/07-decisoes.md#d-001--cada-desenvolvedor-tem-seu-próprio-banco-de-dados-local),
e vale entender: a estrutura do banco é versionada em código (as *migrations*), e código vive em
branch. Se o banco fosse compartilhado, trocar de branch quebraria o ambiente de todo mundo.

### 6.1 Descobrir qual é a sua instância do SQL Server

```powershell
Get-Service | Where-Object { $_.Name -like 'MSSQL*' } | Select-Object Name, Status
```

Compare o resultado com a tabela:

| O que apareceu | O `Server=` que você vai usar |
|---|---|
| `MSSQL$SQLEXPRESS` | `localhost\SQLEXPRESS` |
| `MSSQLSERVER` | `localhost` |
| Nada apareceu, mas você tem LocalDB | `(localdb)\MSSQLLocalDB` |
| Nada apareceu e não tem LocalDB | Instale o SQL Express (§4) |

### 6.2 Criar o arquivo de configuração

O arquivo se chama **`appsettings.Development.json`** e fica em:

```
ProjetoVendasTeste\backend\src\Host\Vendas.Host\appsettings.Development.json
```

Ele **não vem no repositório** — está no `.gitignore` de propósito, porque a configuração é de cada
máquina. O que vem é um modelo, `appsettings.Development.example.json`, ao lado dele.

Copie o modelo:

```powershell
Copy-Item backend\src\Host\Vendas.Host\appsettings.Development.example.json backend\src\Host\Vendas.Host\appsettings.Development.json
```

### 6.3 Colar a string de conexão

Abra o arquivo recém-criado no VS Code e ajuste a linha `"Padrao"`. O conteúdo final deve ser:

```json
{
  "ConnectionStrings": {
    "Padrao": "Server=localhost\\SQLEXPRESS;Database=ProjetoVendasTeste;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Trocando `localhost\\SQLEXPRESS` pelo que você descobriu no passo 6.1. Repare na **barra dupla**:
em JSON, `\` precisa ser escrito `\\`.

O que cada parte significa:

| Parte | O quê |
|---|---|
| `Server=` | Onde o SQL Server está |
| `Database=ProjetoVendasTeste` | O nome do banco. **Não precisa criar antes** — o sistema cria |
| `Trusted_Connection=True` | Entra com o seu usuário do Windows, sem senha |
| `TrustServerCertificate=True` | Aceita o certificado local do SQL. Sem isto, dá erro de certificado |

> ⚠️ **Nunca commite este arquivo.** Ele tem a configuração da sua máquina; se entrar no
> repositório, quebra o ambiente de todo mundo. O `.gitignore` já impede — mas saiba por quê.

### 6.4 Criar o banco

Não precisa fazer nada: **na primeira vez que o backend subir, ele cria o banco e aplica as
migrations sozinho.** Vá para o §7.

Se precisar recriar do zero um dia, apague o banco pelo SQL Server Management Studio e rode o
backend de novo.

---

# 7. Rodar o sistema

São dois processos, em **dois terminais separados**. Os dois precisam ficar abertos.

### Terminal 1 — backend

```powershell
cd backend; dotnet run --project src/Host/Vendas.Host
```

Espere aparecer `Now listening on: http://localhost:5080`.

Confira abrindo <http://localhost:5080/health> no navegador — deve responder `{"situacao":"ok"}`.

A documentação da API fica em <http://localhost:5080/scalar> — lá dá para ver e testar cada
endpoint sem passar pela tela.

### Terminal 2 — frontend

```powershell
cd frontend; npm install; npm run dev
```

O `npm install` só é necessário na primeira vez (e quando alguém adicionar uma dependência nova).

Abra <http://localhost:5173>.

> Se a porta 5173 estiver ocupada, o Vite sobe em 5174 e avisa no terminal. Funciona igual — o
> backend aceita qualquer porta de localhost.

### Conferir que está tudo certo

1. Cadastrar um produto
2. Lançar uma compra com ele
3. Lançar uma venda com ele
4. Ver os totais na tela de Resumo

---

# 8. O fluxo de trabalho

## O ciclo em uma frase

**Toda mudança no sistema nasce de uma issue, é feita numa branch própria, entra por pull request,
é revisada por outra pessoa e só então vira parte do produto.**

```
issue  →  branch  →  commits  →  pull request  →  review  →  merge  →  branch deletada
```

Nada entra por push direto. Nem uma linha, nem do dono do repositório.

## Commits

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

> **Por que importa:** o histórico é a única documentação que nunca fica desatualizada. Quando
> alguém perguntar "por que essa linha existe?", `git blame` só responde se a mensagem do commit
> responder. E o CHANGELOG de uma release se escreve a partir dos commits.

## Pull Request

O [template](.github/pull_request_template.md) já vem preenchido e pede quatro coisas:

| Campo | Para quê |
|---|---|
| **O quê** | O que o PR faz, em 1–3 frases |
| **Por quê** | `Closes #12` — qual issue isso fecha |
| **Como testar** | Passo a passo para o revisor **rodar e validar**. Não é opcional |
| **Decisões** | O que você escolheu quando havia mais de um caminho, e por quê |

**Tamanho:** alvo de até ~400 linhas alteradas. PR maior que isso não é revisado — é carimbado. Se
não couber, quebre em dois (backend primeiro, frontend depois, por exemplo).

## Merge

| De → para | Estratégia | Por quê |
|---|---|---|
| `feature/*`, `fix/*` → `develop` | **Squash and merge** | Os commits da branch viram um só. O histórico da `develop` fica com um commit por funcionalidade |
| `release/*`, `hotfix/*` → `main` | **Merge commit** | Preserva o histórico e deixa explícito no grafo que houve uma publicação |

Depois do merge, a branch é deletada — o GitHub oferece o botão.

## Release e hotfix

**Release** — quando a `develop` acumula um conjunto que faz sentido publicar: branch
`release/1.0.0` a partir da `develop`, CHANGELOG, PR para `main` com merge commit, tag `v1.0.0`, e
**merge de volta em `develop`**.

**Hotfix** — bug em produção que não pode esperar: branch `hotfix/{n}-...` a partir da **`main`**
(não da `develop`, senão levaria junto funcionalidades ainda não publicadas), correção + o teste
que reproduz o bug, PR para `main`, tag `v1.0.1`, e **merge de volta em `develop`** — senão o bug
reaparece na próxima release.

Comandos completos em [docs/03-fluxo-git.md](docs/03-fluxo-git.md) §6 e §7.

---

# 9. Trabalhando com o Claude Code

Usamos o Claude Code para escrever código. Isso muda **quem faz cada etapa**, mas não muda o fluxo:
as regras, o CI e a revisão continuam iguais.

| Etapa | Quem faz |
|---|---|
| Decidir o que precisa ser feito (a issue) | **Você** |
| Criar a branch a partir da `develop` atualizada | Claude Code |
| Escrever o código e os testes | Claude Code |
| Rodar build, testes e type-check antes de commitar | Claude Code |
| Commits no formato, com mensagens de verdade | Claude Code |
| **Ler o diff e decidir se está bom** | **Você**, no VS Code |
| Pedir ajustes | **Você** → Claude Code |
| Push e abertura do PR | Claude Code |
| Aprovar e mergear | **Outra pessoa** |

## O que não pode ser delegado

**Ler o diff.** Se o Claude Code escreve e o PR é aprovado sem ninguém ler, o fluxo vira teatro — o
CI e a aprovação existem justamente para haver um humano no caminho.

Você continua sendo responsável pelo código que leva o seu nome no commit.

## Como conversar com ele

Coisas que funcionam bem:

- **"Implemente a issue #2"** — ele lê a issue, cria a branch com o nome certo, implementa,
  roda os testes, commita e abre o PR
- **"Não gostei disso, refaça assim"** — depois de ler o diff
- **"Por que você fez dessa forma?"** — vale perguntar antes de aprovar
- **"Rode os testes"** / **"O CI está vermelho, veja o que é"**

Coisas que **não** funcionam:

- Pedir para ele aprovar ou mergear o próprio PR — o GitHub não permite, e não deveria mesmo
- Pedir para contornar o CI ou as regras do repositório

## Como ler o diff no VS Code

1. Painel **Source Control** (ícone de bifurcação, na barra esquerda)
2. Clicar no arquivo abre o comparativo lado a lado: verde entrou, vermelho saiu
3. Depois do push, a extensão **GitHub Pull Requests** mostra o PR dentro do editor, e você
   comenta linha a linha sem sair dali

## As regras que o Claude Code segue

Estão em [CLAUDE.md](CLAUDE.md) — ele lê esse arquivo antes de qualquer tarefa. Resumo: não inventa
regra de negócio, não muda arquitetura por conta própria, não aumenta o escopo do que foi pedido,
e não faz nada destrutivo sem confirmar.

Se ele fizer algo fora disso, é um bug do prompt — vale ajustar o `CLAUDE.md`, que é versionado
como qualquer outro arquivo.

---

# 10. Um ciclo completo, do começo ao fim

Exemplo real: a issue #2 do repositório, "tela de contas a receber".

### 1. A issue já existe

[Issues → #2](https://github.com/SeklaServices/ProjetoVendasTeste/issues/2). Leia os **critérios de
aceite** — é o contrato do que precisa estar pronto.

Se for criar uma issue nova: **Issues → New issue**, escolher o template (Funcionalidade, Bug ou
Tarefa técnica), preencher. O GitHub não deixa abrir issue em branco de propósito.

### 2. Pedir ao Claude Code

> Implemente a issue #2.

Ele vai: atualizar a `develop`, criar `feature/2-contas-a-receber`, implementar, rodar build e
testes, commitar em passos e avisar você.

### 3. Conferir você mesmo

**Ler o diff** no painel Source Control do VS Code, arquivo por arquivo. Perguntas úteis:

- Faz o que a issue pediu? Nem menos, nem mais?
- Tem alguma regra de negócio num lugar errado (endpoint ou repositório)?
- Tem senha, string de conexão ou segredo no código?
- Tem teste para a regra nova?

**Rodar o sistema** (§7) e conferir os critérios de aceite na tela.

Se algo estiver errado, é agora que se corrige — não depois que o revisor apontar.

### 4. Abrir o PR

Peça ao Claude Code, ou faça pela web: o GitHub mostra a faixa **"Compare & pull request"**.

⚠️ **Confira que o `base` é `develop`, não `main`.** O GitHub às vezes sugere `main`, e é o erro
mais comum de quem está começando.

Em **Reviewers**, escolha a pessoa que vai revisar.

### 5. Esperar duas coisas ficarem verdes

No rodapé do PR:

- **All checks have passed** — o CI. Se ficar vermelho, clicar em **Details** para ver o erro
- **1 approval** — a revisão

Se o CI reprovar, corrija (ou peça ao Claude Code), commite e faça push. O PR atualiza sozinho.

### 6. Merge

**Squash and merge** → **Confirm** → **Delete branch**.

A issue #2 fecha sozinha, porque o PR tem `Closes #2`.

### 7. Voltar para a develop atualizada

```powershell
git switch develop; git pull
```

**Nunca comece a próxima tarefa sem isto.** Criar branch a partir de uma `develop` velha é o que
transforma conflito pequeno em conflito grande.

---

# 11. Revisar o código de outra pessoa

Revisar é uma habilidade que se treina, e é onde o conhecimento do sistema se espalha pelo time.
Aqui **todo mundo revisa todo mundo**.

## Como fazer

1. Abrir o PR → aba **Files changed**
2. Ler o diff
3. Para comentar numa linha: passar o mouse e clicar no **`+`** azul à esquerda
4. **Start a review** (não "Add single comment" — assim os comentários saem todos juntos)
5. Ao terminar, botão **Review changes**:
   - **Comment** — observações, sem julgamento
   - **Approve** — pode entrar
   - **Request changes** — tem algo bloqueante

## Classifique cada comentário

| Prefixo | Significado |
|---|---|
| `[bloqueante]` | Precisa mudar antes do merge |
| `[sugestão]` | Melhoraria, mas não bloqueia |
| `[dúvida]` | Não entendi, me explica |
| `[elogio]` | Isso ficou bom — use, review não é só apontar erro |

Comente sobre o código, nunca sobre a pessoa:

> `[bloqueante]` Esse `if` de "compra precisa ter item" está no endpoint. Se alguém chamar o
> handler de outro lugar, a regra não roda. Sobe para `Compra.Criar()`?

## O que verificar

- **Faz o que a issue pediu** — nem menos, nem mais
- **Regra de negócio no lugar certo** — nunca em endpoint nem em repositório
- **Nenhum módulo acessando o `DbContext` de outro**
- **Sem segredo, senha ou string de conexão no código**
- **Regra nova tem teste**
- **Você conseguiu rodar** seguindo o "Como testar" do PR

**Rode o PR antes de aprovar.** No VS Code, troque para a branch dele (nome da branch na barra azul
de baixo) e siga o "Como testar". Aprovar sem rodar é carimbar.

## Como receber um review

Todo comentário merece resposta: ou você muda, ou explica por que não. "Feito" e "resolvi de outro
jeito porque X" são as duas respostas válidas. Silêncio não é uma delas.

Crítica ao código não é crítica a você. Essa é a parte que leva mais tempo para assentar, e é a
mais importante.

---

# 12. As regras que o GitHub aplica sozinho

Tudo acima está **configurado no GitHub**, não apenas escrito aqui. As regras rodam no servidor,
quando o push chega:

```
$ git push origin main
remote: error: GH013: Repository rule violations found for refs/heads/main.
remote: - Changes must be made through a pull request.
```

O que é impossível, não apenas desaconselhado:

| Tentativa | Resultado |
|---|---|
| `git push` direto em `main` ou `develop` | Recusado |
| Merge sem 1 aprovação | Botão de merge bloqueado |
| Merge com CI vermelho | Botão de merge bloqueado |
| Aprovar o próprio PR | O GitHub não oferece a opção |
| Commitar depois de aprovado e mergear | A aprovação é derrubada; precisa de nova |
| Merge com comentário de review em aberto | Bloqueado |
| `git push --force` em branch protegida | Recusado |
| Apagar `main` ou `develop` | Recusado |
| Admin "pular" a regra | `bypass_actors` vazio — nem admin passa |

A configuração está versionada em [`.github/rulesets/`](.github/rulesets/), o que significa que
**mudar as regras também é um PR**: fica no histórico, com autor, data e motivo.

Como aplicar num repositório novo e o que mais dá para ligar (proteção contra segredo commitado,
Dependabot, rulesets no nível da organização):
[docs/08-protecao-do-repositorio.md](docs/08-protecao-do-repositorio.md).

## Por que cada regra existe

| Regra | O que ela evita |
|---|---|
| Nada de push direto em `main`/`develop` | Alguém quebra a base de todo mundo às 18h de sexta |
| Toda mudança passa por PR | Ninguém é a única pessoa que sabe como uma parte funciona |
| CI obrigatório | "Na minha máquina funciona" deixa de ser argumento |
| 1 aprovação obrigatória | Erro óbvio é pego antes de virar bug em produção |
| Branch curta | Conflitos pequenos em vez de gigantes |
| Commit descritivo | `git blame` e `git bisect` viram ferramentas úteis |
| Issue antes do código | A discussão sobre *o que fazer* acontece antes de dois dias de trabalho |

Nenhuma existe por burocracia. Cada uma é resposta a um problema que já aconteceu em algum projeto.

---

# 13. Quando dá problema

## Conflito no PR

O PR mostra **"This branch has conflicts that must be resolved"**.

Não é erro — duas pessoas mudaram a mesma coisa e o git não vai chutar qual está certa.

**Pela web** (funciona bem quando é simples): botão **Resolve conflicts** no PR. Aparece o arquivo
com marcações:

```
<<<<<<< feature/3-contas-a-pagar
{ key: '/contas-a-pagar', label: <Link to="/contas-a-pagar">Contas a pagar</Link> },
=======
{ key: '/contas-a-receber', label: <Link to="/contas-a-receber">Contas a receber</Link> },
>>>>>>> develop
```

Decida o conteúdo final — **neste caso as duas linhas ficam**, porque uma feature adicionou um menu
e a outra adicionou o outro. Apague as três linhas de marcação (`<<<<<<<`, `=======`, `>>>>>>>`),
clique em **Mark as resolved** e **Commit merge**.

**Pelo Claude Code:** peça "resolva o conflito do PR". Ele mostra o que cada lado queria antes de
decidir.

## Erros comuns

| Sintoma | Causa | Solução |
|---|---|---|
| `O token '&&' não é um separador de instruções válido` | PowerShell 5.1 não aceita `&&` | Trocar por `;` |
| `ConnectionStrings:Padrao não configurada` | Falta o `appsettings.Development.json` | §6.2 |
| `A network-related or instance-specific error` | Instância do SQL errada | Conferir o `Server=` — §6.1 |
| `The certificate chain was issued by an untrusted authority` | Falta `TrustServerCertificate=True` | Adicionar na string — §6.3 |
| `dotnet` ou `npm` não encontrado | Terminal aberto antes da instalação | Fechar e reabrir o terminal |
| Frontend não carrega dados | Backend não está rodando | Conferir <http://localhost:5080/health> |
| `Updates were rejected because the remote contains work` | Alguém pushou antes de você | `git pull --rebase` |
| Diff do PR mostra o arquivo inteiro alterado | Fim de linha (CRLF/LF) | Não alterar `core.autocrlf` |
| Build reclama de `NU1903` | Advisory conhecido, sem correção disponível | Esperado — [D-006](docs/07-decisoes.md) |

## Comandos de sobrevivência

| Situação | Comando |
|---|---|
| "Onde eu estou?" | `git status` |
| Ver o histórico visualmente | `git log --oneline --graph --all -20` |
| Desfazer o último commit, mantendo as alterações | `git reset --soft HEAD~1` |
| Desfazer um commit já enviado | `git revert <sha>` — **nunca** `reset` em branch compartilhada |
| Guardar trabalho pela metade | `git stash` … depois `git stash pop` |
| Fiz besteira e quero voltar no tempo | `git reflog` — ele lembra de tudo, inclusive do que você "perdeu" |

Prefere não usar linha de comando? O mesmo fluxo por botões está em
[docs/09-fluxo-sem-linha-de-comando.md](docs/09-fluxo-sem-linha-de-comando.md).

---

# 14. Documentação completa

| # | Documento | O que responde |
|---|---|---|
| 0 | [CLAUDE.md](CLAUDE.md) | Regras do projeto — obrigatório para IA e humanos |
| 1 | [docs/01-escopo-funcional.md](docs/01-escopo-funcional.md) | O que o sistema faz e o que não faz |
| 2 | [docs/02-arquitetura.md](docs/02-arquitetura.md) | Camadas, pastas, modelo de dados, contratos de API |
| 3 | [docs/03-fluxo-git.md](docs/03-fluxo-git.md) | O fluxo em detalhe, com todos os comandos |
| 4 | [docs/04-plano-de-desenvolvimento.md](docs/04-plano-de-desenvolvimento.md) | Os PRs que compõem a v1.0.0 |
| 5 | [docs/05-setup.md](docs/05-setup.md) | Setup em detalhe |
| 6 | [docs/06-exercicios-git.md](docs/06-exercicios-git.md) | 11 exercícios práticos, em 4 níveis |
| 7 | [docs/07-decisoes.md](docs/07-decisoes.md) | Decisões tomadas e o porquê de cada uma |
| 8 | [docs/08-protecao-do-repositorio.md](docs/08-protecao-do-repositorio.md) | Como o GitHub aplica as regras sozinho |
| 9 | [docs/09-fluxo-sem-linha-de-comando.md](docs/09-fluxo-sem-linha-de-comando.md) | O mesmo fluxo, por botões |
