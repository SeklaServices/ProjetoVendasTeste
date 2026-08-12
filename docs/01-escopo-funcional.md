# 01 — Escopo Funcional

**Status:** proposta para validação
**Versão:** 1.0

---

## 1. Objetivo do projeto

Construir um sistema web de vendas **deliberadamente simples**, para que a equipe do escritório
pratique, na prática e com um repositório real, o fluxo de trabalho de desenvolvimento:

- criar branches de feature
- abrir pull requests
- revisar código de outra pessoa
- resolver conflitos de merge
- promover `develop` → `main` via release
- corrigir produção com hotfix

O sistema precisa ser **pequeno o suficiente para caber na cabeça** de qualquer pessoa em 15
minutos, e **grande o suficiente para gerar PRs de verdade** — que tocam backend, frontend, banco e
testes ao mesmo tempo.

### Critério de sucesso

Não é "o sistema funciona". É: **toda pessoa da equipe consegue, sozinha, pegar uma tarefa, criar a
branch, implementar, abrir o PR, receber review, resolver um conflito e ver o merge acontecer.**

---

## 2. O que o sistema faz

### 2.1 Cadastro de Produtos

Cadastro simples, com CRUD completo (criar, listar, editar, excluir).

| Campo | Tipo | Regra |
|---|---|---|
| Código | texto (até 20) | Obrigatório. **Único** no sistema. |
| Nome | texto (até 120) | Obrigatório. |
| Unidade de medida | texto (até 10) | Obrigatório. Ex.: `UN`, `KG`, `CX`, `L`. |
| Preço de custo | decimal(18,4) | Obrigatório. Não pode ser negativo. |
| Preço de venda | decimal(18,4) | Obrigatório. Não pode ser negativo. |
| Ativo | booleano | Padrão `true`. Produto inativo não aparece na seleção de compras/vendas. |
| Data de cadastro | data/hora | Preenchida pelo sistema. |

**Regras:**
- Não é permitido excluir produto que já foi usado em alguma compra ou venda — nesse caso o
  sistema orienta a inativá-lo.
- Preço de venda menor que o de custo é permitido, mas a tela exibe um aviso (não bloqueia).

### 2.2 Cadastro de Clientes

CRUD completo, como o de produtos. É o cliente da venda — a compra continua com fornecedor em texto
livre.

| Campo | Tipo | Regra |
|---|---|---|
| Código | inteiro | **Gerado pelo sistema**, sequencial e único. Não é digitado nem editável. |
| Nome | texto (até 120) | Obrigatório. |
| CPF / CNPJ | texto (até 18) | Opcional. Aceita com ou sem pontuação — guardado só com dígitos. **Único quando informado.** 11 dígitos (CPF) ou 14 (CNPJ). |
| Telefone | texto (até 20) | Opcional. |
| E-mail | texto (até 120) | Opcional. Verificação simples de formato. |
| Ativo | booleano | Padrão `true`. Cliente inativo não aparece na seleção de vendas. |
| Data de cadastro | data/hora | Preenchida pelo sistema. |

**Regras:**
- Não é permitido excluir cliente que já tem vendas — o sistema orienta a inativá-lo.
- **O dígito verificador do CPF/CNPJ não é validado** — apenas a quantidade de dígitos. Limitação
  conhecida e deliberada (`07-decisoes.md` D-009).

### 2.3 Compras (entrada)

Registro de uma entrada de mercadoria. É um documento com cabeçalho e itens.

**Cabeçalho:**

| Campo | Tipo | Regra |
|---|---|---|
| Número | inteiro | Gerado pelo sistema, sequencial, único. |
| Data da compra | data | Obrigatória. Não pode ser futura. |
| Fornecedor | texto (até 120) | Obrigatório. **Texto livre** — não existe cadastro de fornecedor. |
| Observação | texto (até 500) | Opcional. |
| Valor total | decimal | **Calculado** = soma dos subtotais dos itens. Nunca digitado. |

**Itens (1..N):**

| Campo | Tipo | Regra |
|---|---|---|
| Produto | referência | Obrigatório. Apenas produtos ativos. |
| Quantidade | decimal(18,4) | Obrigatória. Maior que zero. |
| Preço unitário | decimal(18,4) | Obrigatório. Maior ou igual a zero. Sugerido = preço de custo do produto. |
| Subtotal | decimal | **Calculado** = quantidade × preço unitário. |

**Regras:**
- Uma compra precisa ter **pelo menos um item**.
- O mesmo produto pode aparecer em mais de um item da mesma compra.
- Compra **não pode ser editada** depois de salva — apenas excluída e refeita. (Decisão deliberada:
  reduz o escopo e evita discussão de versionamento de documento.)

### 2.4 Vendas (saída)

Estruturalmente idêntica à compra, com nomes e sugestões diferentes.

**Cabeçalho:** Número (sequencial próprio), Data da venda (não futura), **Cliente** (selecionado do
cadastro, obrigatório, precisa estar ativo), Observação, Valor total (calculado).

**Itens:** Produto (ativo), Quantidade (> 0), Preço unitário (sugerido = **preço de venda** do
produto), Subtotal calculado.

**Regras:** mesmas da compra — mínimo um item, sem edição após salvar.

### 2.5 Resumo (tela inicial)

Painel simples, somente leitura, com:

- Total comprado no período
- Total vendido no período
- Diferença (vendas − compras)
- Quantidade de produtos cadastrados / ativos
- Lista das 5 últimas compras e 5 últimas vendas

Filtro por intervalo de datas. Sem gráficos na primeira versão (fica como exercício).

---

## 3. O que o sistema **NÃO** faz

Esta lista é normativa. Nada aqui entra sem uma decisão explícita registrada.

| Fora de escopo | Por quê |
|---|---|
| **Controle de estoque** | Decisão do responsável. Não há saldo, não há validação de disponibilidade, não há movimento de estoque. Uma venda de 100 unidades de um produto que nunca foi comprado é **válida**. |
| Cadastro de **fornecedores** | A compra continua com texto livre. O cadastro de **clientes** passou a existir (D-009). |
| Autenticação, login, permissões | Ambiente local, sem dados reais. Vira exercício opcional. |
| Multi-empresa / multi-filial | Complexidade do sistema real, desnecessária aqui. |
| Fiscal (NF-e, impostos), financeiro (contas a pagar/receber) | Fora do propósito. |
| Edição de compra/venda já salva | Reduz escopo deliberadamente. |
| Relatórios em PDF/Excel, impressão | Exercício futuro. |
| Deploy em produção real | O "deploy" aqui é simbólico: merge em `main` + tag. |

---

## 4. Personas

Só existe um perfil de usuário: **o operador**. Ele cadastra produtos e lança compras e vendas.
Não há hierarquia, aprovação ou segregação de acesso.

---

## 5. Glossário

| Termo | Significado neste projeto |
|---|---|
| **Cliente** | Cadastro de quem compra. Código sequencial gerado pelo sistema. A venda aponta para ele. |
| **Cliente não identificado** | Cliente genérico criado pela migration, que recebeu as vendas anteriores ao cadastro. |
| **Compra** | Documento de entrada de mercadoria. Não afeta estoque (não existe estoque). |
| **Venda** | Documento de saída de mercadoria. Não afeta estoque. |
| **Movimento** | Termo guarda-chuva para compra ou venda. Nome do módulo que contém as duas. |
| **Item** | Linha de um documento (compra ou venda), sempre ligada a um produto. |
| **Subtotal** | Quantidade × preço unitário de um item. Sempre calculado, nunca digitado. |
| **Valor total** | Soma dos subtotais de um documento. Sempre calculado, nunca digitado. |
