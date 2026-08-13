# 07 — Registro de Decisões

Toda decisão que alguém pode questionar daqui a seis meses fica aqui, com o motivo. Formato
enxuto de ADR (*Architecture Decision Record*) — o mesmo hábito do projeto oficial.

---

## D-001 — Cada desenvolvedor tem seu próprio banco de dados local

**Data:** 2026-08-12
**Status:** Aceita
**Contexto da pergunta:** cada um com seu banco, ou um banco de desenvolvimento unificado?

### Decisão

Cada desenvolvedor roda um **SQL Server local, com seu próprio banco**. Não existe banco de
desenvolvimento compartilhado. Bancos compartilhados existem apenas para **ambientes**
(homologação, produção) — nunca para desenvolvimento.

### Justificativa

**1. O schema é versionado no código — e o código vive em branches.**
Este é o motivo principal, e é exatamente o que amarra a decisão ao git. A estrutura do banco não
é "o que está no servidor": é o conjunto de *migrations* commitado. Cada branch de feature pode
ter uma migration diferente.

Com banco compartilhado: você está em `feature/12-produtos`, aplica sua migration nova. Seu colega
está em `feature/15-compras`, na branch dele essa coluna não existe — e o sistema dele quebra, com
um erro que não tem nada a ver com o código dele. Pior: se o PR 12 for rejeitado, a coluna já está
no banco de todo mundo.

Com banco local: cada um aplica as migrations da própria branch. Troca de branch, roda
`dotnet ef database update`, o banco acompanha o código. **O banco vira consequência do git, não
um estado paralelo a ele.**

**2. Trabalho destrutivo fica contido.** Apagar tudo e recomeçar, testar exclusão em massa,
popular com dados absurdos para ver o que quebra — tudo isso é normal em desenvolvimento e
inviável se o banco é de todos.

**3. Você não depende de ninguém para trabalhar.** Sem VPN, sem "o servidor caiu", sem "quem
derrubou o banco?". Máquina nova produz em minutos.

**4. É como o CI funciona.** O GitHub Actions sobe um banco limpo a cada execução. Se o seu
ambiente também parte de um banco reproduzível a partir das migrations, o que passa na sua máquina
passa no CI. Se o seu ambiente é um banco compartilhado com anos de sujeira acumulada, você
descobre a diferença tarde demais.

### O que se perde, e como compensar

O único argumento real a favor do banco compartilhado é ter **uma massa de dados comum e
realista** — todo mundo vendo os mesmos produtos, os mesmos lançamentos. Isso se resolve sem
compartilhar banco:

- Um **script de seed** versionado no repositório, que popula o banco com dados de exemplo. Todo
  mundo roda o mesmo script e tem os mesmos dados — e o script está no git, então evolui por PR
  como qualquer outro código.
- Para o projeto oficial, onde a massa realista importa mais: um **backup restaurável** com dados
  anonimizados, que cada dev restaura na própria máquina.

O segundo argumento — "instalar SQL Server em cada máquina dá trabalho" — se paga uma vez.

### Quando um banco compartilhado é correto

Para **ambientes**, não para pessoas:

| Ambiente | Banco | Quem aplica migration |
|---|---|---|
| Desenvolvimento | Um por desenvolvedor, local | O próprio dev, ao trocar de branch |
| Homologação / staging | Um, compartilhado | O pipeline, ao fazer merge em `develop` |
| Produção | Um | O pipeline, ao fazer merge em `main` |

A progressão espelha o fluxo de branches. Um banco de dev compartilhado não tem lugar nessa
tabela — ele seria um ambiente sem branch correspondente.

---

## D-002 — Sem controle de estoque

**Data:** 2026-08-12
**Status:** ⚠️ **Superada pela [D-008](#d-008--o-sistema-passa-a-controlar-estoque) em 2026-08-12**

> O texto abaixo é o original, mantido intacto. Decisão registrada não se apaga — se supera. Quem
> ler o histórico daqui a um ano precisa entender tanto o que valia antes quanto por que mudou.

Decisão do responsável. O objetivo do projeto é treinar o fluxo de trabalho, não modelar um ERP.
Estoque traria saldo, validação de disponibilidade, custo médio e concorrência — tudo relevante no
sistema real, tudo ruído aqui.

Consequência: uma venda de 100 unidades de um produto nunca comprado é **válida**. Isso não é bug.

---

## D-003 — Compras e vendas no mesmo módulo (`Movimentos`)

**Data:** 2026-08-12
**Status:** Aceita

Poderiam ser dois módulos. Ficaram em um, por dois motivos:

1. São estruturalmente idênticos (cabeçalho + itens, total calculado). Separar duplicaria a
   estrutura sem ensinar nada novo.
2. Duas pessoas trabalhando no mesmo módulo **geram conflitos de merge reais** — que é justamente
   a matéria-prima que o projeto precisa produzir.

---

## D-004 — Sem edição de compra ou venda já salva

**Data:** 2026-08-12
**Status:** Aceita

Só criar e excluir. Editar um documento com itens abre discussão de versionamento, histórico e
estorno — assunto pesado, e sem relação com o objetivo. Se a equipe quiser, vira exercício depois:
é uma boa oportunidade de praticar **reverter uma decisão documentada** (ver `06-exercicios-git.md`).

---

## D-005 — Só testes unitários

**Data:** 2026-08-12
**Status:** Aceita

Sem testes de integração com banco, sem E2E. O que o projeto precisa é de um `dotnet test` rápido
que **fica vermelho quando alguém quebra uma regra** — porque o CI vermelho bloqueando um PR é
parte do que se está treinando. Testes de integração custariam mais setup do que ensinariam.

No projeto oficial eles existem e são obrigatórios. Aqui, não.

---

## D-006 — Conviver com o aviso NU1903 do Microsoft.OpenApi

**Data:** 2026-08-12
**Status:** Aceita, com revisão pendente

O build emite `NU1903`: `Microsoft.OpenApi` 2.x tem advisory conhecido
([GHSA-v5pm-xwqc-g5wc](https://github.com/advisories/GHSA-v5pm-xwqc-g5wc)). O pacote entra
transitivamente por `Microsoft.AspNetCore.OpenApi` 10.0.0, e ainda não há versão 2.x corrigida. A
3.x foi testada e **não** compila com o source generator do ASP.NET Core 10 (`error CS0200`).

**Decisão:** manter o aviso **visível**. Não usar `NoWarn` nem `NuGetAuditMode` para escondê-lo —
suprimir alerta de segurança é pior que conviver com ele sabendo. A documentação da API (Scalar) só
é exposta em `Development`, e o projeto não vai a produção.

**Revisar quando:** sair uma 2.x corrigida, ou o ASP.NET Core passar a suportar a 3.x. É um bom
primeiro PR de `chore(deps)` para alguém da equipe.

---

## D-007 — Sem chave estrangeira entre módulos

**Data:** 2026-08-12
**Status:** Aceita

`ComprasItens.ProdutoId` e `VendasItens.ProdutoId` apontam para `Produtos`, mas **não têm FK** — só
índice.

**Motivo:** `Produtos` pertence ao `CadastrosDbContext` e os itens ao `MovimentosDbContext`. Uma FK
entre eles obrigaria a migration de um módulo a conhecer a tabela do outro, o que quebraria a
independência que a arquitetura inteira existe para manter — as duas migrations passariam a ter
ordem obrigatória de aplicação.

**Como a integridade é garantida sem a FK:**
- Na criação: `ValidacaoProdutosDosItens` recusa item cujo produto não existe ou está inativo
- Na exclusão: `ExcluirProdutoCommandHandler` consulta `IConsultaMovimentos` e recusa excluir
  produto já usado

É uma troca consciente: perde-se a garantia do banco, ganha-se modularidade — e ganha-se uma
mensagem de erro decente em vez de uma violação de constraint. Essa mesma troca aparece no projeto
oficial e é o tipo de decisão que precisa estar escrita, senão parece esquecimento.

---

## D-009 — Cliente vira cadastro, e a venda aponta para ele

**Data:** 2026-08-12
**Status:** Aceita
**Issue:** #12

> **Sobre a numeração:** esta decisão saiu como D-009, e não D-008, porque o número D-008 estava
> reservado à decisão de estoque, que na época ainda não tinha sido mergeada. As duas entraram, e
> por isso a D-008 aparece **depois** desta no arquivo — a ordem cronológica de merge não é a
> ordem numérica. Foi essa reserva que evitou dois D-008 diferentes.

O campo `Cliente` da venda era texto livre. `Venda.ClienteId` passa a apontar para um cadastro de
clientes, em `Cadastros`.

O problema que isso resolve: "Padaria Central", "padaria central" e "Padaria Central LTDA" eram
três clientes distintos, e nada podia ser agrupado por cliente.

### As decisões

**A. Quais campos o cliente tem** → `Codigo`, `Nome`, `Documento`, `Telefone`, `Email`, `Ativo`,
`DataCadastro`.

Documento, telefone e e-mail são **opcionais**. O documento é **único quando informado** — é o que
impede o cadastro duplicado que motivou a issue. No banco isso é um índice único **filtrado**
(`WHERE [Documento] IS NOT NULL`): sem o filtro, o SQL Server aceitaria apenas um cliente sem
documento.

**Limitação assumida: o CPF/CNPJ não tem o dígito verificador validado** — só a quantidade de
dígitos (11 ou 14). Validar DV é regra com muitos casos de borda e não é o foco do projeto. Existe
um teste (`Documento_invalido_no_digito_verificador_e_aceito`) que **documenta** essa decisão: se
alguém implementar a validação, o teste fica vermelho e a conversa acontece no PR.

O e-mail é verificado de forma simples: arroba no meio, ponto no domínio, sem espaços. Consequência
assumida: endereços de intranet sem ponto (`fulano@servidor`) são recusados — num cadastro de
clientes isso quase sempre é erro de digitação.

**B. O que fazer com as vendas que já existiam** → um cliente genérico.

A migration cria o **"Cliente não identificado"**, com um `Id` fixo
(`Vendas.Shared.ClientesConhecidos.NaoIdentificado`), e aponta todas as vendas existentes para ele.

O nome que estava digitado **não se perde**: vai para a observação da venda, como
`Cliente original: Mercado Central`.

O `Id` fixo existe por um motivo concreto: a migration de `Movimentos` precisa apontar para um
registro criado pela migration de `Cadastros`, e são dois `DbContext` diferentes. Um id constante,
declarado em `Vendas.Shared`, é a única forma de ligá-las sem uma consultar a tabela da outra.

**C. A venda guarda só o `ClienteId`** — não guarda o nome do momento da venda.

Consequência: **renomear um cliente renomeia em todo o histórico**. Um ERP de verdade guardaria um
*snapshot* do nome, porque a nota fiscal precisa refletir quem era o cliente naquela data. Aqui não
há nota fiscal, documento não se edita (D-004) e a coluna a mais confundiria mais do que ajudaria.

**D. O código do cliente é gerado pelo banco**, por `SEQUENCE` (`SeqCliente`), como o número da
compra e da venda. O usuário não digita e não altera. Diferente de `Produto`, cujo código é
digitado — ali o código costuma vir do fornecedor ou do setor, aqui não vem de lugar nenhum.

**E. Cliente inativo não recebe venda.** Mesmo comportamento de produto inativo: a tela não oferece,
e o backend recusa com 422 mesmo que alguém chame a API direto.

**F. O contrato da API mudou.** `POST /api/v1/vendas` recebia `"cliente": "texto"` e passa a receber
`"clienteId": "guid"`. As respostas trazem `clienteId`, `clienteCodigo` e `clienteNome`.

É *breaking change*, e foi aceito porque o único consumidor é o frontend deste repositório, alterado
no mesmo PR.

### Como Movimentos enxerga o cliente

Pelo contrato `IConsultaClientes`, em `Vendas.Shared` — implementado por `Cadastros`, injetado em
`Movimentos`. Mesmo padrão de `IConsultaProdutos`. Nenhum acesso ao `CadastrosDbContext`.

O caminho inverso (impedir excluir cliente com vendas) usa `IConsultaMovimentos.ClienteUtilizadoAsync`,
porque **não existe FK entre os módulos** ([D-007](#d-007--sem-chave-estrangeira-entre-módulos)).
A issue #12 pedia "FK `RESTRICT`, igual a produto" — mas produto também não tem FK, e a garantia
sempre esteve na camada Application.

### Fora de escopo

Cadastro de fornecedores (a compra continua com texto livre), importação por planilha, relatório
por cliente, endereço, e qualquer campo fiscal.

---

## D-008 — O sistema passa a controlar estoque

**Data:** 2026-08-12
**Status:** Aceita
**Supera:** [D-002](#d-002--sem-controle-de-estoque)

O sistema passa a controlar o estoque dos produtos. Compras aumentam o saldo, vendas diminuem, e
a posição é consultável.

O projeto continua sendo, antes de tudo, um campo de treino do fluxo de trabalho. O estoque entra
porque dá material de trabalho realista para a equipe — não porque o objetivo mudou.

### As sete perguntas

**1. Onde o estoque mora?** → **Módulo `Estoque` próprio.**

Não dentro de `Movimentos`. Custa mais — `DbContext` próprio, migration própria, comunicação por
contrato — e é justamente por isso que foi escolhido: é o único ponto do projeto onde a
modularidade é exercitada de verdade, com um módulo novo entrando na estrutura existente. Espelha
o módulo `Inventory` do CeasaSystemNext.

**2. Como o saldo é calculado?** → **Somando os movimentos na consulta.**

Existe a tabela `MovimentosEstoque` (um registro por entrada ou saída) e **não** existe tabela de
saldo. O saldo é `SUM(quantidade)` filtrado por produto.

O CeasaSystemNext mantém uma tabela de saldo (`SaldoEstoqueFisico`) porque lá o volume justifica.
Aqui não justifica, e a versão sem tabela de saldo tem uma vantagem que importa mais: **é
impossível dessincronizar**. Não existe o bug clássico de "o saldo diz 40 e os movimentos dizem
37" porque só existe uma fonte da verdade.

Se um dia o volume pesar, a tabela de saldo entra como otimização — e aí é uma decisão nova, com
o histórico já registrado.

**3. Existe tabela de movimento de estoque?** → **Sim, é a única tabela.**

Cada registro guarda: produto, tipo (entrada/saída), quantidade, data, e a origem (tipo do
documento, id e número). É o que responde "por que o saldo está nesse número".

**4. Excluir compra/venda estorna o saldo?** → **Sim.**

Excluir o documento apaga os movimentos de estoque gerados por ele. Como o saldo é a soma dos
movimentos, o estorno é consequência automática — não existe um "movimento de estorno" separado.

Coerente com a [D-004](#d-004--sem-edição-de-compra-ou-venda-já-salva): documento não se edita, se
exclui e refaz.

**5. Venda sem saldo: bloqueia, avisa ou permite?** → **Avisa, mas permite.**

A venda é gravada. A resposta da API traz um campo `avisos`, e a tela mostra a mensagem depois de
salvar:

> Estoque insuficiente: 'BAN001 — Banana Prata' tinha 70 KG e a venda usou 100 KG.

O motivo é operacional: bloquear a venda por causa de um saldo que pode estar errado (lançamento
atrasado, compra ainda não digitada) trava o faturamento por um problema de cadastro. Avisar dá a
informação sem parar quem está vendendo.

**6. Saldo negativo pode existir?** → **Sim**, é consequência direta da 5.

Não é erro nem inconsistência: significa que saiu mais do que entrou **no que foi registrado**.
A tela mostra em vermelho para chamar atenção, e o histórico permite descobrir a origem.

**7. E os documentos já lançados?** → **Recalcular a partir do histórico.**

A migration inicial do módulo gera os movimentos de estoque a partir das compras e vendas que já
existem no banco. Assim o saldo nasce coerente com o que as telas já mostram, e todo desenvolvedor
vê a mesma coisa — sem dependência de quem lançou o quê antes.

### Consequência técnica: atomicidade entre dois módulos

Escolher módulo próprio (pergunta 1) traz um problema que não existiria dentro de `Movimentos`:
gravar a venda e gravar o movimento de estoque passam a ser **dois `SaveChanges`, em dois
`DbContext` diferentes**. Sem cuidado, dá para existir venda sem a saída de estoque correspondente
se o processo morrer no meio.

**Solução:** os dois `SaveChanges` acontecem dentro de um `TransactionScope`
(`System.Transactions`), com `TransactionScopeAsyncFlowOption.Enabled`. Como os dois `DbContext`
apontam para o mesmo banco físico e usam a mesma string de conexão, o SQL Server trata como uma
transação local — não escala para transação distribuída.

Isto é o preço da modularidade, e está escrito para que ninguém descubra sozinho depois.

### Consequência de processo: novo escopo de commit

Passa a existir o escopo **`estoque`** para commits e issues. Os escopos válidos passam a ser:
`cadastros`, `movimentos`, `estoque`, `host`, `frontend`, `ci`, `docs`, `deps`.

### Fora de escopo (continua não existindo)

Ajuste ou inventário manual, custo médio, valorização, depósitos, lotes, validade, reserva, e
tratamento de concorrência entre duas vendas simultâneas do mesmo produto — esta última fica
registrada como **limitação conhecida**: com o saldo somado na consulta e sem bloqueio, duas vendas
ao mesmo tempo simplesmente geram dois movimentos, e o saldo reflete os dois.
