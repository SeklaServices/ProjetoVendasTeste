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
**Status:** Aceita

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
