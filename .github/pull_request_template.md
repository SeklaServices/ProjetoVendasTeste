<!--
Título do PR no formato de commit: tipo(escopo): descrição
Exemplo: feat(cadastros): cadastro de produtos
-->

## O quê

<!-- O que este PR faz, em 1 a 3 frases. Descreva o comportamento, não os arquivos. -->

## Por quê

Closes #

<!-- Se não fecha uma issue, explique aqui o motivo do PR. -->

## Como testar

<!--
Passo a passo para o revisor validar RODANDO. Seja específico — o revisor não sabe o que
você sabe.

1. `dotnet run --project backend/src/Host/Vendas.Host`
2. Abrir http://localhost:5173/produtos
3. Clicar em "Novo produto", preencher código "ABC" e salvar
4. Tentar salvar outro produto com o mesmo código → deve exibir "Código já cadastrado"
-->

1.
2.
3.

## Decisões

<!--
O que você escolheu quando havia mais de um caminho, e por quê. Se não houve decisão
relevante, escreva "Nenhuma".
-->

## Checklist do autor

- [ ] Faz o que a issue pediu — nem menos, nem mais
- [ ] Regra de negócio está no Domain/Application (não em endpoint nem em repositório)
- [ ] Nenhum módulo acessa o `DbContext` de outro
- [ ] Teste unitário para toda regra de negócio nova
- [ ] `dotnet build` e `dotnet test` verdes localmente
- [ ] `npm run type-check` e `npm run build` verdes localmente
- [ ] Nenhum segredo, senha ou string de conexão no código
- [ ] Nenhum arquivo de configuração local commitado (`appsettings.Development.json`, `.env.local`)
- [ ] Termo de negócio novo adicionado ao glossário (`docs/01-escopo-funcional.md` §5)
- [ ] Branch atualizada com a base (sem conflito)

## Para o revisor

<!-- Opcional: onde você quer atenção especial, o que está em dúvida. -->
