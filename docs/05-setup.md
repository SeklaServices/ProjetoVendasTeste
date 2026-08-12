# 05 — Setup do Ambiente

**Status:** proposta para validação
**Versão:** 1.0

---

## 1. Pré-requisitos

| Ferramenta | Versão | Verificar com | Onde obter |
|---|---|---|---|
| .NET SDK | **10.x** | `dotnet --version` | https://dotnet.microsoft.com/download |
| Node.js | **24.x** | `node --version` | https://nodejs.org |
| Git | 2.4x+ | `git --version` | https://git-scm.com |
| GitHub CLI (`gh`) | 2.x | `gh --version` | https://cli.github.com — **não está instalado na máquina atual** |
| SQL Server | 2019+ ou LocalDB | `sqlcmd -S localhost -Q "SELECT @@VERSION"` | Developer Edition ou SQL Express |
| EF Core Tools | 10.x | `dotnet ef --version` | `dotnet tool install --global dotnet-ef` |

> **`gh` é opcional** — dá para abrir PR pela interface web do GitHub. Mas ele economiza muito
> tempo (`gh pr create`, `gh pr checkout`, `gh pr view`) e vale instalar em todas as máquinas.

---

## 2. Clonar e configurar

```bash
git clone https://github.com/SeklaServices/ProjetoVendasTeste.git
cd ProjetoVendasTeste
```

### Identidade no git (uma vez por máquina)

```bash
git config --global user.name "Seu Nome"
git config --global user.email "voce@sekla.com.br"
git config --global pull.rebase true
git config --global init.defaultBranch main
```

`pull.rebase true` faz `git pull` rebasear em vez de criar merge commit — mantém o histórico limpo.

### Fim de linha no Windows

O `.gitattributes` do repositório já normaliza isso (`* text=auto eol=lf`). **Não** mexa em
`core.autocrlf` manualmente — se o diff de um PR vier com o arquivo inteiro alterado, é sinal de
que alguém mexeu.

---

## 3. Banco de dados

Cada desenvolvedor tem **seu próprio banco local**. Nada é compartilhado.

### 3.1 Configurar a string de conexão

`backend/src/Host/Vendas.Host/appsettings.Development.json` **está no `.gitignore`** — é o arquivo
onde cada um coloca a sua conexão. O repositório versiona apenas o exemplo:

```bash
cp backend/src/Host/Vendas.Host/appsettings.Development.example.json \
   backend/src/Host/Vendas.Host/appsettings.Development.json
```

Conteúdo (ajuste o `Server` para a sua instância):

```json
{
  "ConnectionStrings": {
    "Padrao": "Server=localhost;Database=ProjetoVendasTeste;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Para LocalDB: `Server=(localdb)\\MSSQLLocalDB;Database=ProjetoVendasTeste;Trusted_Connection=True`.

> **Por que isso importa para o treino de git:** configuração local nunca vai para o repositório.
> Se alguém commitar `appsettings.Development.json` com a conexão da própria máquina, o PR quebra
> para todo mundo. É o exemplo mais didático de "por que existe `.gitignore`".

### 3.2 Criar o banco

```bash
cd backend
dotnet ef database update --project src/Modulos/Cadastros/Vendas.Cadastros.Infrastructure --startup-project src/Host/Vendas.Host
dotnet ef database update --project src/Modulos/Movimentos/Vendas.Movimentos.Infrastructure --startup-project src/Host/Vendas.Host
```

Duas migrations separadas porque são dois `DbContext` — um por módulo.

---

## 4. Rodar

### Backend

```bash
cd backend
dotnet run --project src/Host/Vendas.Host
```

API em `https://localhost:7001`, Swagger em `https://localhost:7001/swagger`.

### Frontend

```bash
cd frontend
npm install
npm run dev
```

Aplicação em `http://localhost:5173`. A URL da API vem de `frontend/.env.local` (também
gitignorado):

```
VITE_API_URL=https://localhost:7001/api/v1
```

---

## 5. Antes de abrir um PR — rode isto

O CI vai rodar exatamente estes comandos. Rodar antes economiza um ciclo de PR vermelho:

```bash
cd backend && dotnet build && dotnet test
cd ../frontend && npm run type-check && npm run build
```

---

## 6. Problemas comuns

| Sintoma | Causa provável | Solução |
|---|---|---|
| `A network-related or instance-specific error` | Instância do SQL errada na conexão | Conferir `Server=` no `appsettings.Development.json` |
| `The certificate chain was issued by an untrusted authority` | Falta `TrustServerCertificate=True` | Adicionar na string de conexão |
| `dotnet ef` não encontrado | Tool não instalada | `dotnet tool install --global dotnet-ef` |
| CORS bloqueando o frontend | Origem não liberada | Conferir a política de CORS no `Program.cs` |
| Diff do PR mostra o arquivo inteiro alterado | Fim de linha (CRLF/LF) | Não alterar `core.autocrlf`; refazer o arquivo |
| `Updates were rejected because the remote contains work` | Alguém pushou antes de você | `git pull --rebase` e resolver |
