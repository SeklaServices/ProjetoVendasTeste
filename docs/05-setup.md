# 05 — Setup do Ambiente

**Versão:** 2.0

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

```powershell
Copy-Item backend\src\Host\Vendas.Host\appsettings.Development.example.json backend\src\Host\Vendas.Host\appsettings.Development.json
```

Conteúdo (ajuste o `Server` para a sua instância):

```json
{
  "ConnectionStrings": {
    "Padrao": "Server=localhost\\SQLEXPRESS;Database=ProjetoVendasTeste;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Descubra a sua instância com:

```powershell
Get-Service | Where-Object { $_.Name -like 'MSSQL*' } | Select-Object Name, Status
```

| Serviço | `Server=` |
|---|---|
| `MSSQL$SQLEXPRESS` | `localhost\SQLEXPRESS` |
| `MSSQLSERVER` | `localhost` |
| LocalDB (não aparece como serviço) | `(localdb)\MSSQLLocalDB` |

> **Por que isso importa:** configuração local nunca vai para o repositório.
> Se alguém commitar `appsettings.Development.json` com a conexão da própria máquina, o PR quebra
> para todo mundo.

### 3.2 Criar o banco

Na primeira execução, **o próprio backend aplica as migrations** e cria o banco. Basta rodar a API
(passo 4). Se preferir criar antes, ou se precisar recriar:

```bash
cd backend
dotnet ef database update --project src/Modulos/Cadastros/Vendas.Cadastros.Infrastructure
dotnet ef database update --project src/Modulos/Movimentos/Vendas.Movimentos.Infrastructure
```

Dois comandos porque são dois `DbContext` — um por módulo, cada um com sua tabela de histórico
(`__EFMigrationsHistory_Cadastros` e `__EFMigrationsHistory_Movimentos`) no mesmo banco.

O `dotnet ef` não usa o `appsettings.Development.json`: ele usa as factories de design-time, que
leem a variável de ambiente `VENDAS_CONEXAO` e caem no padrão `Server=localhost` se ela não
existir. Se a sua instância for outra:

```powershell
$env:VENDAS_CONEXAO = "Server=localhost\SQLEXPRESS;Database=ProjetoVendasTeste;Trusted_Connection=True;TrustServerCertificate=True"
```

---

## 4. Rodar

### Backend

```bash
cd backend
dotnet run --project src/Host/Vendas.Host
```

API em `http://localhost:5080`. Documentação interativa em `http://localhost:5080/scalar` (só em
Development). Teste rápido: `http://localhost:5080/health` deve responder `{"situacao":"ok"}`.

### Frontend

```bash
cd frontend
npm install
npm run dev
```

Aplicação em `http://localhost:5173`. A URL padrão da API já aponta para `localhost:5080`; para
mudar, copie `frontend/.env.example` para `frontend/.env.local` (gitignorado) e ajuste:

```
VITE_API_URL=http://localhost:5080/api/v1
```

---

## 5. Antes de abrir um PR — rode isto

O CI vai rodar exatamente estes comandos. Rodar antes economiza um ciclo de PR vermelho:

```powershell
cd backend; dotnet build; dotnet test
```

```powershell
cd frontend; npm run type-check; npm run build
```

> **Windows PowerShell 5.1 não aceita `&&`.** Use `;` para encadear comandos. Se você usa o
> PowerShell 7+ (`pwsh`) ou o Git Bash, `&&` funciona — mas `;` funciona em todos, então é o que a
> documentação usa.

---

## 6. Problemas comuns

| Sintoma | Causa provável | Solução |
|---|---|---|
| `O token '&&' não é um separador de instruções válido` | Windows PowerShell 5.1 não suporta `&&` | Trocar por `;`, ou usar `pwsh` / Git Bash |
| `ConnectionStrings:Padrao não configurada` | Falta o `appsettings.Development.json` | Copiar do `.example.json` (§3.1) |
| `A network-related or instance-specific error` | Instância do SQL errada na conexão | Conferir `Server=` no `appsettings.Development.json` |
| `The certificate chain was issued by an untrusted authority` | Falta `TrustServerCertificate=True` | Adicionar na string de conexão |
| `dotnet ef` não encontrado | Tool não instalada | `dotnet tool install --global dotnet-ef` |
| CORS bloqueando o frontend | Origem não liberada | Conferir a política de CORS no `Program.cs` |
| Diff do PR mostra o arquivo inteiro alterado | Fim de linha (CRLF/LF) | Não alterar `core.autocrlf`; refazer o arquivo |
| `Updates were rejected because the remote contains work` | Alguém pushou antes de você | `git pull --rebase` e resolver |
