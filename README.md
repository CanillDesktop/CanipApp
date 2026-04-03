# CanilApp — Guia de configuração do ambiente (onboarding)

Este documento descreve como clonar o repositório e rodar o **CanilApp** localmente, com base no código atual da solução (`CanilApp.sln`). O projeto é composto por um **backend** ASP.NET Core 8, um **frontend** .NET MAUI com **Blazor WebView**, e uma biblioteca **Shared** com modelos/DTOs compartilhados.

---

## 1. Pré-requisitos

### 1.1 SDKs e ferramentas

| Item | Versão / observação | Onde aparece no projeto |
|------|---------------------|-------------------------|
| **.NET SDK** | **8.0** | `Backend/Backend.csproj` → `net8.0`; `Frontend/Frontend.csproj` → `net8.0-windows10.0.19041.0` |
| **Visual Studio 2022** (recomendado) | Com workload **.NET Multi-platform App UI development** (MAUI) | Frontend é MAUI + Blazor |
| **Git** | Qualquer versão recente | — |

**Node.js não é necessário** para este repositório: não há `package.json` no frontend; `Backend/libman.json` existe mas a lista de bibliotecas está vazia.

### 1.2 AWS CLI

- **Não é obrigatório** só para compilar o projeto.
- **Recomendado** se a equipe padronizar credenciais via `aws configure` (perfil em `%UserProfile%\.aws\credentials`), pois o backend usa o SDK AWS com **cadeia padrão de credenciais** para operações que exigem IAM (ver secção 7 e serviços externos).

### 1.3 Banco de dados

- O **Entity Framework Core** está configurado com **SQLite** em arquivo local.
- Caminho fixo no código: pasta `%LocalAppData%\CanilApp\`, arquivo **`canilapp.db`** (ver `Backend/Program.cs`).
- **Não é necessário instalar MySQL/SQL Server** para o fluxo atual do EF Core. Chaves `ConnectionStrings` em `appsettings*.json` **não são lidas** por nenhum `.cs` do backend (não há `GetConnectionString` no código); tratam-se de resquício/configuração legada — pode ignorá-las para rodar o app.

### 1.4 Outras ferramentas (migrations)

- **CLI do EF Core**: pacote `Microsoft.EntityFrameworkCore.Design` e `Microsoft.EntityFrameworkCore.Tools` já estão no `Backend.csproj`. Comandos típicos:
  - `dotnet ef migrations add Nome --project Backend`
  - `dotnet ef database update --project Backend`

---

## 2. Configuração do projeto

### 2.1 Clonar o repositório

```powershell
git clone <URL_DO_REPOSITORIO>
cd CanipApp
```

(A pasta pode chamar `CanipApp` ou outro nome, conforme o remoto.)

### 2.2 Restaurar dependências

Na raiz da solução:

```powershell
dotnet restore CanilApp.sln
```

Ou abra `CanilApp.sln` no Visual Studio e deixe o restore rodar automaticamente.

### 2.3 Arquivos de configuração ignorados pelo Git

O `.gitignore` do repositório **ignora** (entre outros):

- `Backend/appsettings.json`
- `Backend/appsettings.*.json` (inclui `appsettings.Development.json`)
- `Backend/Properties/launchSettings.json`
- `*.env`

**Implicação:** após um `git clone` limpo, **não virá** `appsettings.json` do backend. Cada desenvolvedor precisa **criar o arquivo localmente** (ou usar User Secrets — ver abaixo).

### 2.4 Criar `Backend/appsettings.json`

Crie o arquivo `Backend/appsettings.json` (não commitado) com a estrutura mínima que o código exige.

**Exemplo (substitua pelos valores reais fornecidos pelo time / AWS):**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "AWS": {
    "Region": "us-east-1",
    "UserPoolId": "us-east-1_XXXXXXXXX",
    "ClientId": "abcdefghijklmnopqrstuvwx",
    "IdentityPoolId": "us-east-1:xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
  }
}
```

| Campo | Uso no código |
|--------|----------------|
| `AWS:Region` | Região dos serviços AWS; usada em `CognitoService`, JWT Bearer (`Authority` / issuer Cognito), clientes DynamoDB/Cognito registrados em `Program.cs`. |
| `AWS:UserPoolId` | User Pool do Cognito; JWT e troca de tokens com Identity Pool. |
| `AWS:ClientId` | App client do User Pool (`USER_PASSWORD_AUTH`, `SignUp`, refresh). |
| `AWS:IdentityPoolId` | Identity Pool para credenciais temporárias (`GetTemporaryCredentialsAsync` em `CognitoService`). |
| `ConnectionStrings:DefaultConnection` | **Não utilizada** pelo `Program.cs` atual; pode ficar vazia. |

**Como conferir no código:** leituras em `Backend/Program.cs` (`AWS:Region`, `AWS:UserPoolId`, `AWS:ClientId`) e em `Backend/Services/CognitoService.cs` (todos os quatro campos AWS).

### 2.5 Alternativa: User Secrets (Backend)

O `Backend.csproj` define `UserSecretsId`. Você pode guardar a mesma informação sem arquivo JSON no disco do repositório:

```powershell
cd Backend
dotnet user-secrets init
dotnet user-secrets set "AWS:Region" "us-east-1"
dotnet user-secrets set "AWS:UserPoolId" "us-east-1_XXXXXXXXX"
dotnet user-secrets set "AWS:ClientId" "seu-client-id"
dotnet user-secrets set "AWS:IdentityPoolId" "us-east-1:xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
```

(User Secrets são locais à máquina do desenvolvedor e não vão para o Git.)

### 2.6 Configurar AWS CLI (opcional porém útil)

Se o backend precisar chamar APIs AWS com permissões de IAM (cadastro com `AdminConfirmSignUp`, DynamoDB, etc.):

```powershell
aws configure
```

Informe Access Key, Secret Key e região padrão alinhada a `AWS:Region`.

---

## 3. Configuração do banco (SQLite + EF Core)

### 3.1 Onde fica o arquivo

- Diretório: `%LocalAppData%\CanilApp\`
- Arquivo: `canilapp.db`

O `Program.cs` cria a pasta se não existir e registra o `DbContext` com:

`UseSqlite($"Data Source={dbPath}")`.

### 3.2 Aplicar migrations

As migrations versionadas estão em `Backend/Migrations/` (ex.: `20251129235413_InitialCreate`).

Com o backend como projeto de startup implícito:

```powershell
cd Backend
dotnet ef database update
```

Isso cria/atualiza `canilapp.db` no caminho usado em tempo de design/execução (LocalApplicationData).

### 3.3 Migrations automáticas na subida da API

No `Program.cs`, o bloco que chama `db.Database.Migrate()` está **comentado**. Ou seja, **subir o backend não aplica migrations sozinho** — é preciso rodar `dotnet ef database update` (ou descomentar conscientemente esse bloco em ambiente controlado).

### 3.4 Alternativa manual

Não há scripts SQL versionados para SQLite além das migrations EF. Para “zerar” o banco: feche o app/backend, apague `%LocalAppData%\CanilApp\canilapp.db` (e `-shm`/`-wal` se existirem) e rode `dotnet ef database update` de novo.

### 3.5 Script `Corrigir-Banco-E-Migrations.ps1`

Existe um script na raiz que apaga o banco local, remove migrations e recria `InitialCreate`. Ele contém **caminho absoluto** de máquina (`C:\Users\Arthu\...`). **Ajuste a variável `$projectPath`** antes de usar, ou prefira os comandos `dotnet ef` acima.

---

## 4. Configuração do AWS Cognito

### 4.1 Onde obter User Pool ID e App Client ID

1. Console AWS → **Amazon Cognito** → **User pools** → selecione o pool do projeto.
2. **User pool ID** → valor do tipo `us-east-1_xxxxxxxxx` → mapeia para `AWS:UserPoolId`.
3. Aba **App integration** → **App clients** → **Client ID** → mapeia para `AWS:ClientId`.

### 4.2 Identity Pool ID

1. Console AWS → **Cognito** → **Federated Identities** (Identity pools) → selecione o pool vinculado ao mesmo projeto.
2. Copie o **Identity pool ID** (formato `região:uuid`) → `AWS:IdentityPoolId`.

### 4.3 Valores “globais” do time

Em ambiente de desenvolvimento compartilhado, **User Pool, App Client e Identity Pool** costumam ser os **mesmos** para todos os devs (recurso AWS único), enquanto **credenciais IAM** (quem pode chamar `AdminConfirmSignUp`, DynamoDB, etc.) são por pessoa ou por perfil `aws configure`.

### 4.4 Fluxo de autenticação no app

- Login: `USER_PASSWORD_AUTH` com `ClientId` (`CognitoService.AuthenticateAsync`).
- JWT na API: validação contra o issuer `https://cognito-idp.{region}.amazonaws.com/{userPoolId}` (`Program.cs`).
- **Importante no App Client:** deve estar habilitado o fluxo **ALLOW_USER_PASSWORD_AUTH** (ou equivalente), senão o Cognito retorna erro interpretado no código como configuração/credenciais inválidas.

### 4.5 Cadastro (SignUp + confirmação admin)

`RegisterUserAsync` chama `SignUpAsync` e em seguida **`AdminConfirmSignUpAsync`**. Essa última exige **credenciais AWS (IAM)** com permissão adequada no Cognito. Sem isso, o cadastro pela API pode falhar mesmo com User Pool correto.

### 4.6 Criar usuário de teste

**Opção A — pelo app:** use a tela de cadastro, se o IAM e o Cognito estiverem configurados.

**Opção B — pelo console Cognito:** User pool → **Users** → **Create user** (enviar convite ou definir senha conforme política do pool).

Garanta atributos usados pelo código, por exemplo **email**, **name** e **`custom:permissao`** (valores alinhados a `PermissoesEnum` em `Shared` — ver `CognitoService` para o parse).

---

## 5. Executar o projeto

### 5.1 Fluxo “oficial” (Frontend sobe o Backend)

O `Frontend/MauiProgram.cs` chama `BackendStarter.StartBackendAndGetUrl()`, que:

1. Procura `Backend.exe` ou `Backend.dll` em pastas relativas ao executável (incluindo `Backend\` copiado no output).
2. Inicia o processo com **`--urls http://127.0.0.1:0`** (porta **aleatória**).
3. Lê a URL em `%LocalAppData%\CanilApp\backend.json` ou na linha `BACKEND_URL:...` no stdout.

O `Frontend.csproj` inclui o target **`CopyBackendAfterBuild`**: após compilar o Frontend em `net8.0-windows10.0.19041.0`, compila o Backend e copia a saída para a pasta `Backend\` ao lado do app.

**No Visual Studio:** defina **Frontend** como projeto de inicialização e execute (Windows). Na primeira execução, confira o Output se o backend foi copiado e iniciado.

### 5.2 Rodar só o Backend (API)

Útil para depurar API ou Swagger sem MAUI:

```powershell
cd Backend
dotnet run
```

Sem `--urls` na linha de comando, o `Program.cs` usa `http://127.0.0.1:0` (porta dinâmica). Para porta fixa, por exemplo:

```powershell
dotnet run -- --urls http://127.0.0.1:5005
```

Endpoints úteis (código em `Program.cs`):

- `GET /` — status simples
- `GET /api/health` — health check
- Em **Development**: Swagger UI (rota padrão `/swagger`)

Variável de ambiente típica:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
```

### 5.3 URLs de acesso

- **Frontend:** aplicativo desktop Windows (MAUI); não há URL fixa de navegador para a UI principal.
- **Backend:** `http://127.0.0.1:<porta>/` — a porta aparece no console (`BACKEND_URL:...`) ou dentro de `%LocalAppData%\CanilApp\backend.json`.

### 5.4 Swagger

Somente com `ASPNETCORE_ENVIRONMENT=Development` (veja `Program.cs`: `if (app.Environment.IsDevelopment())`).

---

## 6. Problemas comuns

| Sintoma | Causa provável | O que fazer |
|---------|----------------|-------------|
| Backend não inicia: exceção `AWS:... não configurada` | Falta `appsettings.json` ou User Secrets | Criar JSON local ou `dotnet user-secrets set` para todas as chaves `AWS:*`. |
| Erro ao logar no Cognito; mensagem sobre fluxo / parâmetro inválido | App client sem **USER_PASSWORD_AUTH** | Ajustar o App Client no console Cognito (ver comentários em `CognitoService`). |
| Cadastro falha com erro AWS | `AdminConfirmSignUp` sem credenciais IAM | Configurar `aws configure` ou variáveis de ambiente AWS; revisar políticas IAM. |
| `401` / token inválido nas APIs | Issuer ou pool incorreto; relógio do PC; token expirado | Conferir `UserPoolId` e `Region`; sincronizar horário; novo login. |
| “Backend não encontrado” no Frontend | Pasta `Backend` não copiada no output | Dar **Build** no Frontend (target copia o Backend) ou publicar conforme `Frontend.csproj`. |
| Timeout ao subir backend | Antivírus, porta bloqueada, ou crash na inicialização | Ver logs em `%LocalAppData%\CanilApp\logs\backend-*.log` (Serilog em `Program.cs`). |
| CORS | Origem não permitida | A política `LocalhostOnly` em `Program.cs` aceita `localhost`, `127.0.0.1`, `::1` e IPs privados `192.168.*` / `10.*`. Ajuste a origem ou a política se usar outro host. |
| Erro SQLite / tabelas inexistentes | Migrations não aplicadas | `dotnet ef database update` no projeto `Backend`. |
| `taskkill` no build | Target `KillBackendBeforeBuild` no `Frontend.csproj` mata `Backend.exe` antes do build | Normal no Windows; feche instâncias antigas se o build reclamar. |

---

## 7. Segurança

- **`appsettings.json` e `appsettings.Development.json` não devem ir para o Git** (estão no `.gitignore`) para evitar vazamento de IDs, segredos ou strings de conexão.
- **`.gitignore`** também cobre `*.env`, `bin/`, `obj/`, arquivos de usuário do VS, etc.
- **`ConnectionStrings`**: mesmo vazias no exemplo, cada dev pode manter dados locais sem commitar; hoje o EF **não** usa essa chave.
- **Credenciais AWS:** use perfil local ou variáveis de ambiente; não commite keys.
- **Cognito User/Client/Pool IDs** não são “segredos” de alto nível como senhas, mas ainda assim o time optou por não versionar `appsettings` — peça os valores por canal seguro interno.

### Variáveis de ambiente (referência)

O código **não** lê variáveis custom `CANILAPP_*`. Para AWS, vale o padrão do SDK:

- `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`, `AWS_SESSION_TOKEN` (opcional)
- `AWS_PROFILE`, `AWS_REGION` / `AWS_DEFAULT_REGION`

Para ASP.NET:

- `ASPNETCORE_ENVIRONMENT` (ex.: `Development`)

---

## 8. Estrutura do projeto

| Pasta / projeto | Função |
|-----------------|--------|
| **Backend** | API ASP.NET Core 8: controllers, `Services` (Cognito, sync DynamoDB, negócio), `Repositories`, `Context` (EF SQLite), migrations. |
| **Frontend** | .NET MAUI + Blazor WebView: `ViewModels`, `Services` (`BackendStarter`, autenticação, HTTP), UI Razor em `Components` / páginas. Inicia e encerra o processo do Backend em desenvolvimento/distribuição empacotada. |
| **Shared** | Biblioteca `net8.0` com modelos, DTOs e enums compartilhados entre Backend e Frontend (sem dependências pesadas de UI). |

**Serviços externos utilizados (código):**

- **Amazon Cognito** (User Pool + Identity Pool): login, registro, refresh, credenciais temporárias.
- **Amazon DynamoDB**: sincronização de dados (`SyncService` + `IDynamoDBContext`).
- **SQLite** (arquivo local): persistência principal via EF Core.

**Pacotes NuGet relevantes (Backend):** `AWSSDK.CognitoIdentity`, `AWSSDK.CognitoIdentityProvider`, `AWSSDK.DynamoDBv2`, `AWSSDK.Extensions.NETCore.Setup`, `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `Serilog.AspNetCore`, `Swashbuckle.AspNetCore`, entre outros — lista completa em `Backend/Backend.csproj`.

**Pacotes relevantes (Frontend):** `Microsoft.Maui.*`, `Microsoft.AspNetCore.Components.WebView.Maui`, `AWSSDK.CognitoIdentityProvider`, `CommunityToolkit.Mvvm`, `sqlite-net-pcl` (referenciado no `.csproj`) — ver `Frontend/Frontend.csproj`.

---

## Checklist rápido para o primeiro dia

1. Instalar **.NET 8** + **Visual Studio 2022** com workload **MAUI**.
2. `git clone` + `dotnet restore CanilApp.sln`.
3. Criar **`Backend/appsettings.json`** (ou User Secrets) com **`AWS:Region`**, **`UserPoolId`**, **`ClientId`**, **`IdentityPoolId`**.
4. Configurar **credenciais AWS** se for usar cadastro admin e/ou DynamoDB.
5. `cd Backend` → `dotnet ef database update`.
6. Definir **`ASPNETCORE_ENVIRONMENT=Development`** se quiser Swagger ao rodar o Backend isolado.
7. Executar o **Frontend** no Windows e validar login com usuário de teste no Cognito.

Em caso de dúvida sobre um comportamento específico, use **busca no repositório** pelos símbolos citados (`Program.cs`, `CognitoService`, `BackendStarter`, `CanilAppDbContext`).
