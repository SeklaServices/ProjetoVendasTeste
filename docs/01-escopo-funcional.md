# 01 — Escopo Funcional

**Versão:** 2.0

---

## 1. Objetivo do projeto

Sistema web de vendas **deliberadamente enxuto**: cadastro de produtos e clientes, entradas de
compra, saídas de venda e um resumo do período.

O escopo pequeno é uma **escolha de projeto**, não uma limitação. Ela produz três efeitos que
importam:

- **O sistema inteiro cabe na cabeça** de quem chega, em cerca de 15 minutos
- **Todo PR é revisável de verdade**, porque nenhuma mudança precisa ser gigante
- **Toda ausência é explicável**: o que o sistema não faz está registrado com o motivo, em
  [07-decisoes.md](07-decisoes.md)

### O critério que decide o que entra

Uma funcionalidade só entra se **alguém precisa dela para operar**. "Seria bom ter" não é
justificativa suficiente — vira item de backlog em [04-roadmap.md](04-roadmap.md), com o motivo
escrito.

Quando uma decisão de escopo muda, a decisão antiga é **superada**, nunca apagada. Foi o que
aconteceu com o controle de estoque (D-002 → D-008) e com o cliente em texto livre (D-009).

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

### 2.5 Estoque

Cada produto tem um **saldo**, movimentado automaticamente pelos documentos:

- Uma **compra** gera uma **entrada** de estoque para cada item, na quantidade do item
- Uma **venda** gera uma **saída**
- **Excluir** um documento apaga os movimentos que ele gerou — o saldo volta ao que era

**Não existe tabela de saldo.** O saldo é a soma dos movimentos do produto. Isso torna impossível
o saldo divergir do histórico (ver `07-decisoes.md` D-008).

**Regras:**
- Vender mais do que o saldo disponível **é permitido**. A venda é gravada e o sistema **avisa**:
  *"Estoque insuficiente: 'BAN001 — Banana Prata' tinha 70 KG e a venda usou 100 KG."*
- Como consequência, **o saldo pode ficar negativo**. Não é erro: significa que saiu mais do que
  entrou no que foi registrado. A tela mostra em vermelho.
- Compras nunca são bloqueadas nem geram aviso.

**Tela:** posição de todos os produtos com o saldo atual, busca por código ou nome, filtro
"somente com saldo", e o histórico de movimentos de cada produto com a origem de cada um.

### 2.6 Resumo (tela inicial)

Painel simples, somente leitura, com:

- Total comprado no período
- Total vendido no período
- Diferença (vendas − compras)
- Quantidade de produtos cadastrados / ativos
- Lista das 5 últimas compras e 5 últimas vendas

Filtro por intervalo de datas. Sem gráficos — ver o backlog em `04-roadmap.md`.

---

## 3. O que o sistema **NÃO** faz

Esta lista é normativa. Nada aqui entra sem uma decisão explícita registrada.

| Fora de escopo | Por quê |
|---|---|
| Cadastro de **fornecedores** | A compra continua com texto livre. O cadastro de **clientes** passou a existir (D-009). |
| Autenticação, login, permissões | Não há dados sensíveis nem acesso externo. Candidato de backlog. |
| Multi-empresa / multi-filial | Só existe uma operação. Complexidade sem demanda. |
| Fiscal (NF-e, impostos), financeiro (contas a pagar/receber) | Fora do propósito. |
| Edição de compra/venda já salva | Reduz escopo deliberadamente. |
| Relatórios em PDF/Excel, impressão | Candidato de backlog. Exportar CSV vem antes. |
| Deploy automatizado | A publicação é manual: merge em `main`, tag e release. |

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
| **Compra** | Documento de entrada de mercadoria. Gera entrada de estoque. |
| **Venda** | Documento de saída de mercadoria. Gera saída de estoque. |
| **Movimento de estoque** | Um registro de entrada ou saída de um produto, com a origem. É a única fonte do saldo. |
| **Saldo** | Soma dos movimentos de estoque de um produto. Calculado, nunca armazenado. |
| **Movimento** | Termo guarda-chuva para compra ou venda. Nome do módulo que contém as duas. |
| **Item** | Linha de um documento (compra ou venda), sempre ligada a um produto. |
| **Subtotal** | Quantidade × preço unitário de um item. Sempre calculado, nunca digitado. |
| **Valor total** | Soma dos subtotais de um documento. Sempre calculado, nunca digitado. |
