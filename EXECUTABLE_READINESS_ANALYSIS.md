# Análise de prontidão para executável e evolução cloud — CanilApp

Documento baseado **apenas** no código e configuração presentes no repositório (Frontend MAUI + Blazor WebView, Backend ASP.NET Core local, SQLite, AWS Cognito/DynamoDB). Referências principais: `Frontend/MauiProgram.cs`, `Frontend/Services/BackendStarter.cs`, `Backend/Program.cs`, `Backend/Services/CognitoService.cs`, `Backend/Controllers/LoginController.cs`, `Frontend/Handlers/AuthDelegatingHandler.cs`, `Frontend/Frontend.csproj`.

---

## Parte 1 — Diagnóstico do executável local

### Premissas do projeto (confirmadas no código)

- **Frontend:** .NET MAUI 8, alvo **somente** `net8.0-windows10.0.19041.0` (`Frontend/Frontend.csproj`).
- **UI:** Blazor em WebView (`AddMauiBlazorWebView()` em `Frontend/MauiProgram.cs`).
- **Backend:** processo separado (`Backend.exe` ou `Backend.dll` + `dotnet`), iniciado pelo app (`BackendStarter.StartBackendAndGetUrl()`), escuta `http://127.0.0.1:0`, discovery em `%LocalAppData%\CanilApp\backend.json` e linha `BACKEND_URL:` no stdout (`Backend/Program.cs`).
- **Banco:** SQLite em `%LocalAppData%\CanilApp\canilapp.db` (`Backend/Program.cs`).
- **Auth:** login via API → Cognito `USER_PASSWORD_AUTH`; cliente envia **ID Token** como Bearer; API valida JWT com JwtBearer (`Backend/Program.cs`).

### O que já está pronto

| Área | Evidência |
|------|-----------|
| Build do frontend | Projeto SDK MAUI + Razor; `OutputType` Exe. |
| Inicialização do backend | `MauiProgram.CreateMauiApp()` chama `BackendStarter.StartBackendAndGetUrl()` antes do `MauiApp.CreateBuilder()` (`Frontend/MauiProgram.cs`). |
| Cópia do backend no build/publish | Targets `CopyBackendAfterBuild` e `CopyBackendAfterPublish` em `Frontend/Frontend.csproj` (compila/publica Backend e copia para `Backend\` relativo ao output). |
| Persistência local (caminho) | DB e logs sob `LocalApplicationData\CanilApp` (`Backend/Program.cs`). |
| Fluxo de login (happy path) | `POST api/login` → `CognitoService.AuthenticateAsync` → `LoginViewModel` persiste tokens via `CustomAuthenticationStateProvider` / `AuthenticationStateService` (`LoginController`, `CognitoService`, `LoginViewModel`, `AuthenticationStateService`). |
| Refresh no servidor | `POST api/login/refresh` + `CognitoService.RefreshTokenAsync` (`LoginController`, `CognitoService`). |
| Logs do backend (arquivo) | Serilog: `%LocalAppData%\CanilApp\logs\backend-*.log`, retenção 30 dias (`Backend/Program.cs`). |
| Encerramento do backend | `POST /internal/shutdown`, kill switch em `MauiProgram.RegisterKillSwitch()` + `BackendStarter.ShutdownBackend()` (`Backend/Program.cs`, `Frontend/MauiProgram.cs`). |
| Health check | `GET /api/health` usado por `BackendStarter.TestBackendConnection` (`Backend/Program.cs`, `Frontend/Services/BackendStarter.cs`). |

### O que está faltando ou incompleto

| Item | Detalhe técnico |
|------|-----------------|
| **Migrations / schema SQLite** | O bloco que chama `db.Database.Migrate()` está **comentado** em `Backend/Program.cs`. Não há `EnsureCreated` em outros arquivos. Em instalação limpa, o arquivo `canilapp.db` pode existir sem tabelas → falhas nas APIs que usam EF. |
| **Empacotamento runtime do Backend** | `Backend/Backend.csproj`: `<SelfContained>false</SelfContained>` e `RuntimeIdentifier` fixo `win-x64`. O usuário final precisa do **.NET 8 Desktop Runtime** (ou runtime compatível) instalado, **a menos** que publiquem Backend como self-contained (o target `CopyBackendAfterPublish` repassa `SelfContained` do publish do Frontend). |
| **WebView2** | Não há no repositório verificação nem bootstrapper do **WebView2 Runtime**. MAUI Blazor no Windows depende dele; máquinas sem runtime podem falhar ao renderizar a UI. |
| **Configuração AWS para usuário final** | `Backend/appsettings.json` contém `Region`, `UserPoolId`, `ClientId`, `IdentityPoolId`. Valores reais no repo implicam: (1) **ClientId / PoolId não são secretos**, mas amarram o app a um pool específico; (2) **cadastro** via `AdminConfirmSignUpAsync` exige **credenciais IAM** na cadeia padrão da AWS SDK — utilizadores finais típicos **não** têm isso. `appsettings.Development.json` referencia `AWS:Profile` = `default` (máquina do desenvolvedor). |
| **Tratamento de erros de autenticação no cliente** | `AuthDelegatingHandler` lança `HttpRequestException` em **qualquer** status não-sucesso **antes** do bloco que trataria `401` e tentaria refresh — o trecho de refresh após 401 é **código morto** para respostas 401 (ver Parte 2). |
| **Erros amigáveis na inicialização** | Falha em `StartBackendAndGetUrl()` → exceção em `MauiProgram` **antes** da UI; utilizador vê falha brusca (console / crash), sem wizard de diagnóstico. |
| **CORS / origem Blazor WebView** | Política `LocalhostOnly` permite `localhost`, `127.0.0.1`, IPs RFC1918, `::1` (`Backend/Program.cs`). Para backend **local** com cliente apontando para `127.0.0.1`, costuma ser suficiente; se no futuro a origem do WebView for diferente, pode exigir ajuste. |
| **Dependência não usada no Frontend** | Pacote `sqlite-net-pcl` no `Frontend.csproj` sem referências em `.cs` do Frontend — ruído para manutenção, não bloqueia executável. |
| **Target `KillBackendBeforeBuild`** | `taskkill /IM Backend.exe /F` antes de cada build pode afetar **qualquer** `Backend.exe` no sistema (`Frontend/Frontend.csproj`). Relevante para CI/outros projetos na mesma máquina. |

### Riscos

- **JWT expirado / inválido:** validação com `ValidateLifetime = true` (`Backend/Program.cs`); renovação automática no handler **não funciona** como escrito (Parte 2). Utilizador pode precisar voltar ao login manualmente após expiração do ID token.
- **Clock skew / expiração:** `ClockSkew = 5 minutos` mitiga parcialmente diferença de relógio.
- **Audience:** `ValidateAudience = false` — validação menos estrita; aceita tokens cujo `aud` não confere explicitamente ao ClientId (trade-off documentado em comentários vazios perto de `ValidIssuer` em `Backend/Program.cs`).
- **Segurança operacional:** logs do JWT em `OnMessageReceived` registram prefixo do header `Authorization` (`Backend/Program.cs`) — em produção, risco de vazamento parcial de token em ficheiros de log.
- **Sync / DynamoDB:** `SyncController` e `SyncService` usam DynamoDB; cliente DynamoDB configurado com `GetAWSOptions()` / credenciais padrão (`Backend/Program.cs`). Em máquina sem credenciais AWS válidas, sincronização falha (esperado). Endpoint `test-dynamo` usa `new AmazonDynamoDBClient()` sem política mínima explícita no método.
- **Ficheiro `backend.json` órfão:** se um processo antigo morrer sem limpar o ficheiro, outro utilizador na mesma conta Windows poderia teoricamente colidir com PID/porta; o código tenta validar com `/api/health` e reapagar o JSON (`BackendStarter.cs`).

---

## Parte 2 — Análise específica do Cognito

### 1. Como o login é feito

- **Endpoint:** `POST /api/login` com corpo `LoginRequest` (`Login`, `Senha`) — `Backend/Controllers/LoginController.cs`.
- **Fluxo:** `ICognitoService.AuthenticateAsync` → `InitiateAuthAsync` com `AuthFlowType.USER_PASSWORD_AUTH`, `ClientId` de configuração, parâmetros `USERNAME` / `PASSWORD` — `Backend/Services/CognitoService.cs`.
- **Pós-auth:** `GetUserAsync` com **Access Token** para ler atributos (`email`, `name`, `custom:permissao`, `sub`).

### 2. Onde os tokens são guardados

- **MAUI `SecureStorage`:** chaves `id_token`, `access_token`, `refresh_token`, `auth_token` (duplicado com ID token), `user_email`, `user_name` — `Frontend/Services/AuthenticationStateService.cs`.
- **Preferences:** `user_role`, `user_email`, `user_name` após login — `Frontend/ViewModels/LoginViewModel.cs`.

### 3. Como o JWT é enviado para a API

- `AuthDelegatingHandler` anexa `Authorization: Bearer {id_token}` para pedidos que **não** são `/api/login` — `Frontend/Handlers/AuthDelegatingHandler.cs` (comentário explícito a preferir ID token por causa do `aud`).

### 4. Como o backend valida o token

- `AddJwtBearer` com `Authority` / `ValidIssuer` = `https://cognito-idp.{region}.amazonaws.com/{userPoolId}`, `ValidateAudience = false`, `ValidateLifetime = true` — `Backend/Program.cs`.
- `AWS:Region`, `AWS:UserPoolId`, `AWS:ClientId` são **obrigatórios** na configuração na arranque (exceção se ausentes) — mesmas linhas em `Program.cs`.
- **Nota:** `CognitoService` instancia `AmazonCognitoIdentityProviderClient` e `AmazonCognitoIdentityClient` diretamente com `RegionEndpoint` (`CognitoService.cs`), não via `AddAWSService` injetado no construtor — o registo DI de `IAmazonCognitoIdentityProvider` existe em `Program.cs` mas o serviço de domínio usa clientes próprios.

### Avaliação

| Questão | Conclusão |
|---------|-----------|
| Pronto para produção (end user)? | **Parcialmente.** Login por palavra-passe via API + JWT está implementado; cadastro automático com confirmação admin exige IAM; refresh automático no cliente está **quebrado** (abaixo). |
| Falhas de segurança | ClientId/PoolId em `appsettings.json` commitados: **não são segredo** no modelo OAuth público, mas **amarram** o build a um ambiente. Logs com pré-visualização do token. `ValidateAudience = false` alarga superfície de tokens aceites. |
| Refresh token | Servidor: `RefreshTokenAsync` correto com `REFRESH_TOKEN_AUTH` e preservação do refresh quando Cognito não devolve novo — `CognitoService.cs`. Cliente: lógica de retry em `AuthDelegatingHandler` **nunca corre** para 401 porque `!response.IsSuccessStatusCode` dispara exceção antes (linhas ~56–60 vs ~62–85 em `AuthDelegatingHandler.cs`). |
| ClientId / UserPoolId | Alinhados com o padrão Cognito; o backend deixa de arrancar sem eles. O **ID Token** é o Bearer — coerente com `ValidateAudience = false` e documentação interna em `AUTHENTICATION.md` / `docs/SYSTEM_FLOW.md`. |
| Utilizador final real | Necessário: pool Cognito com `ALLOW_USER_PASSWORD_AUTH`; utilizadores criados ou fluxo de registo que **não** dependa de `AdminConfirmSignUp` sem IAM na máquina; estratégia para **não** depender de `~/.aws/credentials` no PC do cliente para operações só de utilizador; corrigir refresh no cliente ou aceitar re-login frequente conforme TTL do ID token. |

---

## Parte 3 — O que falta para um executável profissional

### Obrigatório (ação direta)

1. **Garantir schema SQLite:** reativar `Migrate()` em arranque seguro, ou documentar/implementar instalação inicial que aplique migrations; sem isso o executável é frágil em primeira execução (`Backend/Program.cs`).
2. **Publish coerente:** publicar Frontend com política clara (`SelfContained` true/false) e alinhar Backend via `CopyBackendAfterPublish` (`Frontend/Frontend.csproj`); testar pasta final com **apenas** `Frontend.exe` + `Backend\` conforme mensagem do target.
3. **Runtime .NET no destino:** se `SelfContained=false` no Backend, exigir instalador/pré-requisito .NET 8; documentar versão exata.
4. **WebView2:** detetar ausência do runtime e redireccionar para instalador Microsoft ou embutir bootstrapper — não está no código atual.
5. **Configuração AWS:** substituir valores de desenvolvimento por configuração por ambiente (ficheiros por tenant, variáveis de ambiente, ou configuração empresarial); **não** colocar chaves secretas IAM em `appsettings` distribuído. ClientId/PoolId/Region podem ser públicos, mas o processo de **cadastro** precisa de modelo claro (ex.: só administrador cria utilizadores no Cognito).
6. **Corrigir pipeline HTTP + refresh:** em `AuthDelegatingHandler`, tratar 401 **sem** lançar antes (ou relançar após refresh com novo request); hoje o refresh está inalcançável em 401.
7. **Erros na inicialização:** capturar falhas de `BackendStarter` e mostrar diálogo com log path (`%LocalAppData%\CanilApp\logs`) em vez de falhar silenciosamente na consola.
8. **Reduzir logs sensíveis:** evitar logar prefixos de `Authorization` em produção (`Backend/Program.cs`).

### Recomendado

- **MSIX / Inno Setup / WiX:** empacotar WebView2 + .NET (se FDD) + pasta `Backend`.
- **Auto-update:** não existe no repo; opcional (Squirrel, MSIX store, etc.).
- **First-run wizard:** pedido de confirmação de pré-requisitos e teste `GET /api/health` com mensagem localizada.
- **Remover ou condicionar** `KillBackendBeforeBuild` a Debug/local apenas.
- **Rever** `DevAuthBypass` (`Frontend/Config/DevAuthBypass.cs`): hoje `SkipLogin` é sempre `false` em Debug e Release; manter assim para builds distribuíveis.

---

## Parte 4 — Migração para backend na cloud

### Estado atual no código

- API e dados locais no mesmo processo; URL base é `127.0.0.1` dinâmica (`BackendStarter`, `BackendConfig`).
- Já existe integração **AWS** no mesmo backend: Cognito (auth), DynamoDB (`SyncService`, `SyncController`), Identity Pool (`CognitoService.GetTemporaryCredentialsAsync`). Ou seja, parte da “cloud” já está acoplada ao serviço local.

### Mudanças necessárias

| Mudança | Detalhe |
|---------|---------|
| Separar API cloud vs local | Deixar de iniciar Kestrel local em `MauiProgram`; apontar `HttpClient` “ApiClient” para URL HTTPS pública; opcionalmente manter modo “embedded” por flag de configuração. |
| Persistência | Hoje negócio em **SQLite** (`CanilAppDbContext`); sync em **DynamoDB**. Cloud full implicaria ou **RDS/Aurora + EF**, ou **DynamoDB como fonte de verdade** com reescrita dos repositórios/serviços que hoje são EF-centric — `SyncService` já é um padrão de sincronização híbrida a estudar. |
| Autenticação remota | JWT continua válido contra o mesmo User Pool; CORS deve permitir origens do WebView ou usar esquema sem CORS (depende de como o WebView identifica origem). Ajustar `LocalhostOnly` (`Backend/Program.cs`) para domínios reais. |
| HTTPS / deployment | Publicar atrás de ALB/API Gateway; certificados TLS; possivelmente **Hosted UI** ou PKCE em vez de `USER_PASSWORD_AUTH` (requisito de segurança/compliance comum em cloud). |
| Segredos | Mover config AWS para Parameter Store / Secrets Manager; nunca distribuir IAM keys com o app cliente. |
| `BackendStarter` / `internal/shutdown` | Tornar opcionais ou remover no cliente cloud-only. |

### Complexidade: **Alta**

**Justificativa:** O domínio está implementado em camadas Controller → Service → **EF + SQLite** em múltiplas entidades; `SyncService` é extenso e acoplado a modelos DynamoDB + EF. Migrar “apenas hospedar” o mesmo assembly na AWS é **média**, mas **substituir** SQLite como sistema de registo único ou expor API pública segura (CORS, HTTPS, hardening Cognito, rate limits já existentes em sync) eleva para **alta**. A duplicação de armazenamento (SQLite + DynamoDB) exige decisão arquitetural explícita antes de cortar o processo local.

---

## Parte 5 — Comparação de complexidade

| Critério | Executável local (Windows) | Backend na cloud |
|----------|----------------------------|------------------|
| **Complexidade** | **Média** — build/publish + pré-requisitos + correções de auth handler + migrations + WebView2. | **Alta** — hospedagem, dados, CORS/HTTPS, segurança Cognito, possível refactor de persistência. |
| **Tempo estimado** | Ordem de **1–3 semanas** para um MVP instalável estável (inclui testes em VM limpa, installer básico, correções acima). | Ordem de **1–3 meses** conforme se mantém SQLite no cliente + API só sync, ou se a API cloud substitui totalmente o SQLite. |
| **Riscos** | Máquinas sem .NET/WebView2; DB sem migrations; refresh JWT; utilizadores sem política Cognito correta. | Custos AWS, modelagem multi-tenant, exposição de API, conformidade (password grant), migração de dados offline-first. |

---

## Parte 6 — Plano de ação

### Fase 1 — Executável distribuível (Windows)

1. Corrigir `AuthDelegatingHandler` para que **401** dispare `TryRefreshTokenAsync` e repita o pedido **sem** consumir o corpo de forma a impedir retry (avaliar clonar `HttpRequestMessage` se necessário).
2. Ativar aplicação de migrations no arranque do Backend (com tratamento de erro e log) ou pipeline de instalação que execute `dotnet ef database update` uma vez.
3. Executar `dotnet publish` no Frontend com `RuntimeIdentifier` win e `SelfContained` definido conscientemente; validar que `PublishDir` contém `Backend\Backend.exe` (ou dll + dotnet) e que `BackendStarter` encontra o ficheiro.
4. Testar em VM/conta Windows **sem** SDK .NET: se falhar, publicar Backend self-contained ou documentar instalador do runtime.
5. Validar WebView2 instalado; adicionar deteção e mensagem ou pré-requisito no installer.
6. Separar config: `appsettings.Production.json` / variáveis de ambiente para Cognito; remover dependência de perfil AWS local para fluxos de utilizador; revisar cadastro (`RegisterUserAsync`) para cenário end-user.
7. Suavizar falha de `MauiProgram` quando o backend não sobe (UI + caminho do log).
8. Opcional: remover ou restringir `KillBackendBeforeBuild`.

### Fase 2 — Cloud (evolução)

1. Extrair contrato da API (OpenAPI já gerado em dev com Swagger) e definir URL base configurável no cliente (`BackendConfig` / ficheiro de config).
2. Implementar deploy do Backend (ECS/Fargate, Elastic Beanstalk, ou App Service) com HTTPS e variáveis de ambiente para Cognito.
3. Rever política CORS e remover restrições só localhost para as origens reais do app.
4. Decidir arquitetura de dados: (A) API cloud + SQLite local apenas cache, (B) RDS + EF na cloud e app mais fino, ou (C) DynamoDB como principal — alinhar com `SyncService` existente.
5. End sharding de segredos (Secrets Manager) e CI/CD; monitorização (CloudWatch).
6. Evoluir autenticação para fluxo recomendado para apps públicos (PKCE/Hosted UI), se política de segurança exigir, mantendo compatibilidade com validação JWT no servidor.

---

## Resumo executivo

O repositório já implementa o **núcleo** de um desktop Windows com **API local**, **SQLite**, **Cognito** e **cópia automatizada do Backend** no build/publish. Para um **.exe profissional**, os gaps mais críticos no código são: **migrations desativadas**, **refresh JWT no cliente logicamente inoperante**, **pré-requisitos WebView2/.NET**, e **modelo de configuração/cadastro AWS** adequado a máquinas sem credenciais de desenvolvedor. A ida para **cloud** é **mais trabalhosa** porque a lógica de negócio está acoplada ao SQLite/EF local e à sincronização DynamoDB já existente — exige decisão de arquitetura de dados e endurecimento de rede/segurança, não apenas “subir o mesmo exe”.
