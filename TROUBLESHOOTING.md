# CanilApp — Troubleshooting

Guia prático de problemas frequentes nesta solução (Backend ASP.NET Core 8, Frontend MAUI + Blazor WebView, SQLite, AWS Cognito/DynamoDB). Detalhes de versões e pacotes: `docs/DEPENDENCIAS.md`; onboarding completo: `README.md`.

---

## Referência rápida (Problema | Causa | Solução)

### 1. Backend não inicia

| Problema | Causa provável | Solução |
|----------|----------------|---------|
| Exceção ao subir: `AWS:Region não configurada` (ou `UserPoolId` / `ClientId`) | `appsettings.json` ausente ou sem chaves obrigatórias; clone limpo não traz esse arquivo (`.gitignore`) | Criar `Backend/appsettings.json` com `AWS:Region`, `AWS:UserPoolId`, `AWS:ClientId` (e `IdentityPoolId` para fluxos Cognito completos) ou usar `dotnet user-secrets set` para cada chave (ver `README.md`). |
| Backend encerra logo após iniciar pelo MAUI | Processo filho sem config na pasta de trabalho; `appsettings.json` não copiado para a saída do build | Garantir `appsettings.json` no projeto Backend e **Rebuild** do Backend + Frontend (o target copia `Backend\bin\...\**` para `Frontend\...\Backend\`). |
| `Log.Fatal` / “Backend falhou ao iniciar” genérico | Exceção durante `WebApplication.CreateBuilder` / registro de serviços | Abrir `%LocalAppData%\CanilApp\logs\backend-*.log` (Serilog em `Program.cs`) e o console; corrigir a exceção indicada (config, AWS, etc.). |
| Porta dinâmica não aparece / timeout ao subir | Antivírus, firewall local, ou crash antes de escrever `backend.json` | Conferir logs; fechar processos órfãos; apagar `%LocalAppData%\CanilApp\backend.json` se estiver inconsistente; testar `cd Backend` + `dotnet run -- --urls http://127.0.0.1:5005` para porta fixa. |
| `dotnet run` no Backend sem Swagger | Ambiente não é Development | `ASPNETCORE_ENVIRONMENT=Development` antes de `dotnet run` (Windows PowerShell: `$env:ASPNETCORE_ENVIRONMENT = "Development"`). |

---

### 2. Erros de configuração AWS

| Problema | Causa provável | Solução |
|----------|----------------|---------|
| Chamadas Cognito/DynamoDB falham com credenciais | SDK usa a **cadeia padrão** de credenciais; sem perfil/variáveis | `aws configure` ou variáveis `AWS_ACCESS_KEY_ID` / `AWS_SECRET_ACCESS_KEY` (e opcionalmente `AWS_PROFILE`, `AWS_REGION`). |
| `Region` errada | `AWS:Region` em `appsettings` diferente da região real do User Pool / DynamoDB | Alinhar `AWS:Region` ao console AWS (mesma região dos recursos). |
| DynamoDB ou APIs IAM retornam acesso negado | Usuário/role IAM sem políticas para a tabela/recursos usados pelo `SyncService` | Revisar políticas no IAM para a conta configurada localmente. |
| Cadastro (`SignUp`) ok mas confirmação falha | `AdminConfirmSignUpAsync` exige **permissão IAM** no Cognito | Conceder ao IAM usado localmente a ação equivalente a admin no User Pool (ex.: `cognito-idp:AdminConfirmSignUp` no pool correto). |

---

### 3. Problemas com JWT / Cognito

| Problema | Causa provável | Solução |
|----------|----------------|---------|
| `401` nas APIs após login | Token errado, expirado, issuer inválido ou relógio do PC | Conferir `UserPoolId` e `Region` no backend; sincronizar horário do Windows; fazer login de novo. O `AuthDelegatingHandler` envia o **ID token** como `Bearer` — não trocar por access token sem alinhar a validação no `Program.cs`. |
| Logs `[JWT] Autenticação falhou` | Issuer do token não bate com `https://cognito-idp.{region}.amazonaws.com/{userPoolId}` | Corrigir pool/região na config; garantir que o token é do mesmo User Pool. |
| Login retorna “configuração inválida” / `InvalidParameterException` | App client sem fluxo **USER_PASSWORD_AUTH** | No Cognito → App client → habilitar autenticação por usuário/senha (`ALLOW_USER_PASSWORD_AUTH` / fluxo compatível). |
| Refresh falha / “sessão expirada” | Refresh token revogado, app client sem refresh, ou política do pool | Novo login; conferir configuração do App Client e políticas de token no pool. |
| Erro ao obter credenciais temporárias (Identity Pool) | `AWS:IdentityPoolId` incorreto, pool não vinculado ao User Pool, ou **Id token** inválido para o mapeamento | Conferir Identity Pool no console; ver mapeamento de provedor Cognito; checar se `GetTemporaryCredentialsAsync` recebe Id token do mesmo pool (`CognitoService.cs`). |
| Exceção ao resolver `CognitoService`: `IdentityPoolId` nulo | Config incompleta | Definir `AWS:IdentityPoolId` em `appsettings` ou User Secrets (construtor de `CognitoService` exige os quatro campos AWS). |

---

### 4. Problemas com SQLite e migrations (Backend)

| Problema | Causa provável | Solução |
|----------|----------------|---------|
| Erros do tipo “no such table” / EF falha ao query | Migrations **não** aplicadas na subida: `Database.Migrate()` está **comentado** em `Program.cs` | Na pasta `Backend`: `dotnet ef database update` (cria/atualiza `%LocalAppData%\CanilApp\canilapp.db`). |
| `dotnet ef` não encontrado | Ferramenta global ou pacote não disponível no contexto | Instalar `dotnet tool install --global dotnet-ef` ou usar o pacote `Microsoft.EntityFrameworkCore.Tools` já referenciado no `Backend.csproj` a partir do diretório do projeto. |
| “Database is locked” / timeouts SQLite | Outro processo com o arquivo aberto (segundo Backend, ferramenta, IDE) | Encerrar instâncias duplicadas do Backend; fechar viewers do `.db`; repetir o comando. |
| Banco corrompido ou estado incoerente com migrations | Testes manuais, restores parciais | Fechar app/backend, apagar `canilapp.db` (e `-wal`/`-shm` se existirem) em `%LocalAppData%\CanilApp\`, rodar `dotnet ef database update` de novo. |
| `ConnectionStrings:DefaultConnection` parece ignorada | Por desenho atual do projeto | O EF usa caminho fixo em `Program.cs` (`UseSqlite` → `canilapp.db` em LocalAppData); não depende de `GetConnectionString`. |

**Nota:** O Frontend referencia `sqlite-net-pcl` (cache/local). O banco “oficial” da API é o SQLite do Backend no LocalAppData; problemas de UI offline/local são outro arquivo/caminho conforme o código do app.

---

### 5. Frontend (MAUI / Blazor WebView)

| Problema | Causa provável | Solução |
|----------|----------------|---------|
| Exceção “Backend não encontrado” | `Backend.exe` / `Backend.dll` não está ao lado do app nos caminhos que `BackendStarter` procura | Dar **Build** no projeto **Frontend** (target `CopyBackendAfterBuild` compila e copia o Backend). Para publish, usar o fluxo que dispara `CopyBackendAfterPublish`. |
| `backend.json` aponta para processo morto | Encerramento anormal; arquivo obsoleto | Apagar `%LocalAppData%\CanilApp\backend.json` e relançar o app (o starter detecta PID inválido e reinicia). |
| WebView em branco / “WebView2 não inicia” | Ver secção detalhada abaixo — runtime, bloqueios ou falha silenciosa do motor | Seguir **WebView2: janela abre mas a UI fica branca**; `docs/DEPENDENCIAS.md` (runtime). |
| Build reclama de `Backend.exe` / `taskkill` | Target `KillBackendBeforeBuild` encerra processos antes do build | Esperado no Windows; fechar manualmente o Backend se o build falhar por arquivo em uso. |
| Requisições HTTP falham após mudança de porta | API usa porta **aleatória** (`127.0.0.1:0`) salva em `backend.json` | O `MauiProgram` usa a URL retornada por `StartBackendAndGetUrl()`; se testar API manualmente, ler porta atual em `backend.json` ou no console (`BACKEND_URL:...`). |
| `429 Too Many Requests` em endpoints com rate limit | Política `sync-policy` em `Program.cs` (janela fixa por partição) | Reduzir frequência de chamadas ou ajustar limites em desenvolvimento (código), se apropriado. |
| CORS / origem bloqueada | Política `LocalhostOnly` só permite `localhost`, `127.0.0.1`, `::1`, `192.168.*`, `10.*` | Chamar a API a partir dessas origens ou estender a política em `Program.cs`. |

#### WebView2: janela abre mas a UI fica branca

No Windows, o Blazor deste projeto corre **dentro** do **Microsoft Edge WebView2** (`BlazorWebView` em `Frontend/MainPage.xaml` → `HostPage="wwwroot/index.html"`). Não existe um cenário em que “o Blazor está a correr” com UI visível **sem** o WebView2 estar operacional no cliente: se a zona do browser embutido está só branca, o motor WebView2 não chegou a renderizar conteúdo, falhou na inicialização, ou o conteúdo não carregou.

**Sintomas típicos:** janela MAUI abre (por vezes com fundo branco já definido em `MainPage`), barra de título visível, mas **nada** da aplicação Razor (login, rotas); o backend pode até estar saudável nos logs — isso **não** exclui problema de WebView2.

1. **Runtime WebView2 ausente, desatualizado ou corrompido**  
   - Instalar ou reinstalar o [WebView2 Runtime Evergreen](https://developer.microsoft.com/microsoft-edge/webview2/).  
   - Em **Definições → Aplicações → Aplicações instaladas**, confirmar **Microsoft Edge WebView2 Runtime**.  
   - Máquinas só com Edge “clássico” antigo ou imagens Windows mínimas por vezes não têm o runtime; o instalador Evergreen resolve na maioria dos casos.

2. **Bloqueio por antivírus, EDR ou política de grupo**  
   - Ferramentas corporativas podem bloquear `msedgewebview2.exe` ou pastas de dados do WebView2.  
   - Testar noutro PC, VM limpa, ou utilizador Windows sem políticas restritivas para isolar a causa.

3. **Perfil / pasta de dados do WebView2**  
   - Falhas de permissão ou perfil corrompido podem manifestar-se como ecrã branco sem mensagem clara.  
   - Vale testar outro utilizador no mesmo PC ou, em diagnóstico, limpar dados da aplicação conforme política da equipa.

4. **Confundir com falha só do Blazor ou dos ficheiros `wwwroot`**  
   - Se o motor arranca mas há erro ao carregar recursos ou scripts, o resultado também pode ser branco.  
   - Em depuração no Visual Studio, inspecionar a janela **Saída** e exceções; se tiver acesso às DevTools do WebView2 no fluxo de debug, verificar consola por erros de rede ou JavaScript.

5. **Drivers gráficos / GPU**  
   - Casos raros: atualizar drivers; problemas de aceleração documentados na [documentação WebView2](https://learn.microsoft.com/microsoft-edge/webview2/) podem aplicar-se em hardware muito antigo ou virtualizado.

**O que não resolve sozinho:** copiar apenas o `.exe` para um PC sem WebView2 Runtime — o utilizador precisa do pré-requisito (ou um instalador que embuta o bootstrapper Microsoft, ainda não automatizado neste repositório; ver `EXECUTABLE_READINESS_ANALYSIS.md`).

---

### 6. Dependências faltando

| Problema | Causa provável | Solução |
|----------|----------------|---------|
| Erros de restore NuGet / pacotes não resolvidos | Cache offline, feed indisponível, SDK errado | `dotnet restore CanilApp.sln`; instalar **.NET SDK 8.0**; conferir `Frontend.csproj` / `Backend.csproj`. |
| MAUI não compila / workload ausente | VS sem workload MAUI | Instalar Visual Studio 2022 com **.NET Multi-platform App UI development** (ver `README.md`). |
| Ferramentas EF / scaffolding | Pacote Design/Tools não restaurado | `dotnet restore` no projeto Backend; usar `dotnet ef` a partir da pasta `Backend`. |
| **Node.js** | Não faz parte do build | Não é necessário para este repositório (sem `package.json` no fluxo principal). |

---

## Onde olhar primeiro

| Sintoma | Onde verificar |
|---------|----------------|
| Ecrã branco (WebView) | Pré-requisito WebView2 Runtime (lista de aplicações Windows); antivírus/políticas; secção **WebView2: janela abre mas a UI fica branca** acima; Visual Studio → Saída ao depurar |
| Backend | `%LocalAppData%\CanilApp\logs\backend-*.log`, console, `GET http://127.0.0.1:<porta>/api/health` |
| URL/porta do Backend | `%LocalAppData%\CanilApp\backend.json` ou linha `BACKEND_URL:` no stdout |
| Config obrigatória na API | `Backend/Program.cs` (JWT + SQLite path), `Backend/Services/CognitoService.cs` (Cognito + Identity Pool) |
| Cópia Backend → Frontend | `Frontend/Frontend.csproj` (targets `CopyBackendAfterBuild`, `KillBackendBeforeBuild`) |
