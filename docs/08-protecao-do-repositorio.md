# 08 — Proteção do Repositório (as regras que o Git aplica sozinho)

**Versão:** 2.0

> Regra escrita num documento é combinado. Regra configurada no GitHub é **impedimento**. Este
> documento cobre a segunda.

---

## 1. As três camadas de "regra"

| Camada | Onde vive | Força | Quem contorna |
|---|---|---|---|
| Documentação (README, CLAUDE.md) | Repositório | Combinado | Qualquer um, sem esforço |
| **Rulesets / branch protection** | Configuração do GitHub | **Impedimento no servidor** | Só quem tem permissão de admin — e fica registrado |
| Hooks locais (`.git/hooks`) | Máquina de cada dev | Aviso | Qualquer um, com `--no-verify` |

A camada que importa é a do meio: ela roda **no servidor**, quando o push chega. Não tem como
"esquecer de instalar", não depende de cada máquina, e não se contorna com uma flag.

Hooks locais são úteis para dar feedback rápido (ex.: recusar uma mensagem de commit fora do
padrão antes de o dev perder tempo), mas nunca são a garantia — o servidor é.

---

## 2. O que já está no repositório

| Arquivo | O que é |
|---|---|
| [`.github/rulesets/main.json`](../.github/rulesets/main.json) | Regras da `main`, em JSON pronto para importar |
| [`.github/rulesets/develop.json`](../.github/rulesets/develop.json) | Regras da `develop` |
| [`.github/CODEOWNERS`](../.github/CODEOWNERS) | Quem é convidado automaticamente para revisar |
| [`.github/workflows/ci.yml`](../.github/workflows/ci.yml) | Os checks que os rulesets exigem como obrigatórios |

Os rulesets estarem versionados tem um efeito colateral bom: **mudar as regras vira um PR**. A
alteração fica no histórico, com autor, data e motivo — em vez de alguém mexer numa tela e ninguém
saber quando nem por quê.

> Atenção: versionar o JSON **não aplica** as regras sozinho. O GitHub não lê esses arquivos
> automaticamente — eles são a fonte da verdade e o material de importação. Aplicar é o passo 3.

---

## 3. Como aplicar (uma vez, precisa ser admin do repositório)

1. Abrir **Settings → Rules → Rulesets** no repositório
2. **New ruleset → Import a ruleset**
3. Enviar `.github/rulesets/main.json`
4. Repetir com `.github/rulesets/develop.json`
5. Conferir que ambos aparecem com **Enforcement: Active**

Depois de aplicar, um `git push origin main` responde assim:

```
remote: error: GH013: Repository rule violations found for refs/heads/main.
remote: - Changes must be made through a pull request.
```

Esse é o objetivo. Não é possível "esquecer" da regra.

### Ordem importa

Os checks só podem ser marcados como obrigatórios depois de terem rodado ao menos uma vez — o
GitHub lista os nomes que já viu. Como o CI já rodou no primeiro PR, os dois nomes
(`Backend — Build e Testes` e `Frontend — Type-check e Build`) estarão disponíveis. Se aparecerem
como "não encontrado" na importação, rode um PR qualquer primeiro e importe depois.

---

## 4. O que cada regra faz

| Regra no JSON | Efeito prático |
|---|---|
| `pull_request` | Push direto na branch é recusado. Só entra por PR. |
| `required_approving_review_count: 1` | O PR precisa de 1 aprovação. O GitHub não deixa o autor aprovar o próprio. |
| `dismiss_stale_reviews_on_push` | Novo commit depois da aprovação **derruba** a aprovação. Evita "aprovo agora e mudo depois". |
| `required_review_thread_resolution` | Nenhum comentário de review pode ficar sem resolução. |
| `require_last_push_approval` (só `main`) | Quem fez o último push não pode ser quem aprova. Mais rígido, por ser produção. |
| `required_status_checks` | Merge bloqueado enquanto o CI não estiver verde. |
| `strict_required_status_checks_policy` | A branch precisa estar **atualizada com a base**. É o que força o `rebase` antes do merge. |
| `non_fast_forward` | `git push --force` na branch protegida é recusado. Histórico não se reescreve. |
| `deletion` | A branch não pode ser apagada, nem por acidente. |
| `allowed_merge_methods` | **`main` só aceita merge commit.** `develop` aceita squash (features) e merge commit (reconciliação vinda da `main`). Ver abaixo — essa configuração não é detalhe. |
| `bypass_actors: []` | **Ninguém** contorna — nem admin, nem o dono do repositório. |

### Por que a `main` não aceita squash

Isso não é preferência de estilo — é o que impede um problema real, e aconteceu aqui em 2026-08-13.

**Squash não é merge.** Ele pega os commits da origem, joga fora a ligação com eles, e cria um
commit novo com o conteúdo copiado. Para o git, o resultado não tem nenhum parentesco com a branch
de onde veio.

Para uma branch de feature isso é ótimo: ela morre logo depois, e a `develop` fica com um commit
limpo por funcionalidade.

Para `main` e `develop` é destrutivo. As duas vivem para sempre e precisam continuar se
reconhecendo. Quando a `develop` foi mergeada na `main` com squash, as duas ficaram com **conteúdo
idêntico e histórias separadas** — e o git passou a tratá-las como trabalhos independentes. O
sintoma: todo release seguinte acusa conflito em arquivos que são iguais nos dois lados.

Pior ainda: com as histórias desligadas, qualquer PR que aponte para a `main` passa a ser comparado
com o **primeiro commit do repositório**, e aí *tudo* vira conflito.

Deixar só `merge` na `main` faz o botão errado **deixar de existir**. É a mesma lógica de todo o
resto deste documento: regra que depende de alguém lembrar não é regra.

A tabela completa, que também está no README:

| De | Para | Método |
|---|---|---|
| `feature/*`, `fix/*` | `develop` | Squash |
| `release/*`, `hotfix/*` | `main` | Merge commit |
| `main` de volta para `develop` | `develop` | Merge commit |

### Sobre `bypass_actors` vazio

É deliberado. A exceção que se concede "só desta vez, é urgente" é a que vira hábito. Se um dia for
realmente necessário, um admin pode desativar o ruleset, fazer o que precisa e reativar — e essa
sequência fica no log de auditoria da organização, que é exatamente onde deve ficar.

---

## 5. Outras opções que valem conhecer

### Rulesets no nível da organização

Em **Settings da organização → Rules → Rulesets**, dá para aplicar regras a **todos os
repositórios** de uma vez, com padrão de nome (`repo:*`). A regra passa a existir antes do
repositório, e ninguém precisa lembrar de configurar.

### `push` ruleset — bloquear arquivo por conteúdo

Além do target `branch`, existe o target `push`, que barra o push por **caminho de arquivo**
(`file_path_restriction`) ou tamanho. Útil para impedir que `appsettings.Development.json`, `.env`
ou um `.bak` de 200 MB entrem no repositório mesmo que alguém edite o `.gitignore`.

### Secret scanning e push protection

Em **Settings → Code security**, o `Push protection` recusa o push que contém o que parece ser uma
credencial (token, chave de API, string de conexão com senha). Vale ligar — é gratuito em
repositório privado dentro do GitHub Team/Enterprise, e é a única barreira que age *antes* de o
segredo entrar no histórico. **É a proteção mais importante deste documento.**

### Dependabot

**Settings → Code security → Dependabot alerts / security updates**: abre PR automaticamente quando
uma dependência tem vulnerabilidade conhecida. O time revisa PRs
que não escreveu.

### Hooks locais (opcional)

Para dar feedback antes do push, `commit-msg` validando o formato `tipo(escopo): descrição`:

```bash
#!/bin/sh
padrao='^(feat|fix|test|docs|refactor|chore|ci|perf)\([a-z-]+\): .+'
grep -qE "$padrao" "$1" || {
  echo "Mensagem fora do padrão: tipo(escopo): descrição"
  exit 1
}
```

Vale como conveniência. **Não** vale como garantia: `git commit --no-verify` passa por cima, e o
hook não existe na máquina de quem clonou o repositório (a pasta `.git/hooks` não é versionada).
Ferramentas como Husky resolvem a distribuição, mas não o `--no-verify`.

---

## 6. Checklist de aplicação

- [ ] Ruleset `Protecao main` importado e **Active**
- [ ] Ruleset `Protecao develop` importado e **Active**
- [ ] Testado: `git push origin main` é recusado
- [ ] Testado: PR sem aprovação não libera o botão de merge
- [ ] Testado: PR com CI vermelho não libera o botão de merge
- [ ] "Automatically delete head branches" ligado (Settings → General)
- [ ] Push protection de segredos ligado (Settings → Code security)
- [ ] Time `@SeklaServices/desenvolvedores` existe (senão, ajustar o CODEOWNERS)
