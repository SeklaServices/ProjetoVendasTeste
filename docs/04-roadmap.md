# 04 — Roadmap

**Versão:** 2.0

O que já está entregue, o que está em andamento e o que vem depois.

---

## Entregue — v1.0.0

| # | Entrega | PR |
|---|---|---|
| 1 | Estrutura da solução: 10 projetos backend, 2 de teste, frontend Vite + React + Ant Design, CI | #4 |
| 2 | Cadastro de produtos — CRUD completo, backend e tela | #4 |
| 3 | Compras — documento com cabeçalho e itens, total calculado no domínio | #4 |
| 4 | Vendas — mesma estrutura, com preço sugerido de venda | #4 |
| 5 | Tela de resumo — totais do período e últimas movimentações | #4 |
| 6 | Documentação de onboarding | #5 |

## Entregue — depois da v1.0.0

| Entrega | Issue | PR |
|---|---|---|
| Cadastro de clientes, e venda apontando para o cadastro | #12 | #13 |
| Decisão de escopo do controle de estoque (D-008) | #6 | #17 |

---

## Em andamento

| Entrega | Issue | Depende de |
|---|---|---|
| Saldo de estoque movimentado por compras e vendas | [#7](https://github.com/SeklaServices/ProjetoVendasTeste/issues/7) | D-008, já mergeada |
| Aviso quando a venda usa mais do que o saldo disponível | [#8](https://github.com/SeklaServices/ProjetoVendasTeste/issues/8) | #7 |
| Tela de posição de estoque com histórico de movimentos | [#9](https://github.com/SeklaServices/ProjetoVendasTeste/issues/9) | #7 |
| Auditoria das operações de produtos, compras e vendas | [#10](https://github.com/SeklaServices/ProjetoVendasTeste/issues/10) | — |

As issues #8 e #9 podem ser feitas em paralelo depois da #7. Ambas tocam `Roteador.tsx` e o menu de
`LayoutPrincipal.tsx` — se andarem ao mesmo tempo, a segunda a mergear vai precisar de rebase.

---

## Backlog

Nada aqui está aprovado. São candidatos, e cada um precisa virar issue com critérios de aceite
antes de ser implementado.

| Candidato | Observação |
|---|---|
| Cadastro de fornecedores | A compra ainda usa texto livre. Espelha o que foi feito com clientes (D-009) |
| Exportar listagens em CSV | Pequeno e independente |
| Gráficos na tela de resumo | Exige dependência nova — PR próprio de `chore(deps)` |
| Login com JWT | Atravessa o sistema inteiro. Hoje não há autenticação (`01-escopo-funcional.md` §3) |
| Edição de compra e venda já salvas | Reverte a [D-004](07-decisoes.md). Precisa de decisão registrada antes do código |
| Validação de dígito verificador de CPF/CNPJ | Hoje só o tamanho é verificado — limitação assumida na [D-009](07-decisoes.md) |
| Snapshot do nome do cliente na venda | Hoje renomear um cliente renomeia o histórico ([D-009](07-decisoes.md), decisão C) |

---

## Como uma entrega entra no roadmap

1. Vira uma **issue** com critérios de aceite verificáveis
2. Se mudar uma decisão já registrada, a decisão antiga é **superada** em
   [07-decisoes.md](07-decisoes.md) — nunca apagada — e isso vem **antes** do código
3. É implementada numa branch própria e entra por PR com review

## Definition of Done

- [ ] Faz o que a issue pediu — nem menos, nem mais
- [ ] Regra de negócio no Domain/Application, nunca em endpoint ou repositório
- [ ] Nenhum módulo acessa o `DbContext` de outro
- [ ] Teste unitário para toda regra nova
- [ ] `dotnet build` e `dotnet test` verdes
- [ ] `npm run type-check` e `npm run build` verdes
- [ ] Sem segredo ou string de conexão no código
- [ ] PR com **o quê / por quê / como testar / decisões**
- [ ] CI verde e 1 aprovação
- [ ] Termo de negócio novo no glossário ([01-escopo-funcional.md](01-escopo-funcional.md) §5)
