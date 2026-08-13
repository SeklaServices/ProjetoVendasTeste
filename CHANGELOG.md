# Changelog

Todas as mudanças relevantes de cada versão publicada.

O formato segue [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/), e o versionamento
segue [SemVer](https://semver.org/lang/pt-BR/).

---

## [1.1.0] — 2026-08-13

### Adicionado

- **Cadastro de clientes** — CRUD completo com código sequencial gerado pelo banco, nome, CPF/CNPJ
  opcional (único quando informado), telefone, e-mail e situação. Tela com busca por código, nome
  ou documento (#12, PR #13)
- **`CHANGELOG.md`** e roadmap em `docs/04-roadmap.md`

### Alterado

- **A venda aponta para o cadastro de clientes.** O campo `Cliente`, que era texto livre, virou
  `ClienteId`. A tela troca o campo de digitação por seleção, e a listagem exibe `código — nome`
  vindo do cadastro (#12, PR #13)
- **Contrato da API de vendas** — `POST /api/v1/vendas` passa a receber `clienteId` no lugar de
  `cliente`. As respostas trazem `clienteId`, `clienteCodigo` e `clienteNome`
  **⚠️ Breaking change.** O único consumidor é o frontend deste repositório, alterado no mesmo PR
- **Documentação** reescrita para descrever o sistema pelo que ele é, sem o enquadramento de
  projeto de treino que existia até aqui (PR #20)

### Decisões registradas

- **D-008** — o sistema passará a controlar estoque, em módulo próprio, com o saldo somado dos
  movimentos e aviso (não bloqueio) quando a venda exceder o disponível. Supera a **D-002**.
  A implementação está nas issues #7, #8 e #9 (#6, PR #17)
- **D-009** — cliente vira cadastro; a venda guarda só o id, sem snapshot do nome (#12, PR #13)

### Corrigido

- **Histórico da `main` e da `develop` reconciliado.** A v1.0.0 foi publicada com *squash*, o que
  desfez a ligação entre as duas branches: elas ficaram com conteúdo idêntico e histórias separadas,
  e todo PR passou a acusar conflito falso. A ligação foi restabelecida (PR #19)
- **`main` passa a aceitar apenas merge commit.** O botão de squash, que causou o problema acima,
  deixou de ser oferecido. A `develop` passa a aceitar squash (features) e merge commit
  (reconciliação vinda da `main`) (PR #18)
- Migração de dados: as vendas que existiam antes do cadastro de clientes foram atribuídas ao
  cliente **"Cliente não identificado"**, e o nome que estava digitado foi preservado na observação
  do documento

### Limitações conhecidas

- O dígito verificador de CPF/CNPJ **não** é validado — apenas a quantidade de dígitos (D-009)
- Renomear um cliente renomeia o nome exibido em **todo o histórico** de vendas: a venda guarda o
  id, não um snapshot do nome (D-009, decisão C)
- O build emite `NU1903` para `Microsoft.OpenApi` 2.x. Não há versão corrigida disponível, e a 3.x
  é incompatível com o ASP.NET Core 10. O aviso é mantido visível de propósito (D-006)

---

## [1.0.0] — 2026-08-12

### Adicionado

- **Cadastro de produtos** — CRUD completo com código único, nome, unidade de medida, preço de
  custo e de venda
- **Compras** — documento com cabeçalho (data, fornecedor em texto livre) e itens. Valor total
  calculado no domínio, nunca digitado
- **Vendas** — mesma estrutura, com preço unitário sugerido a partir do preço de venda do produto
- **Tela de resumo** — totais de compras e vendas do período, diferença, contagem de produtos e as
  últimas movimentações
- **Backend** .NET 10 com Minimal API e Clean Architecture em dois módulos (`Cadastros` e
  `Movimentos`), um `DbContext` por módulo e comunicação entre módulos apenas por contrato em
  `Vendas.Shared`
- **Frontend** React 19 + TypeScript strict + Vite + Ant Design 6 + TanStack Query
- **CI** no GitHub Actions: build e testes do backend, type-check e build do frontend
- **Rulesets** versionados em `.github/rulesets/`, templates de issue e de PR
- Documentação de escopo, arquitetura, fluxo de trabalho, setup e decisões

[1.1.0]: https://github.com/SeklaServices/ProjetoVendasTeste/releases/tag/v1.1.0
[1.0.0]: https://github.com/SeklaServices/ProjetoVendasTeste/releases/tag/v1.0.0
