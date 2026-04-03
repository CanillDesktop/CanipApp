# Fluxo do sistema (execução) — CanilApp

Este documento descreve **como a aplicação roda na prática**, usando **nomes reais** de classes e arquivos do repositório. É um guia de onboarding: visão geral, sem entrar em cada regra de negócio.

---

## Visão geral (uma frase)

O **Frontend** (.NET MAUI + Blazor) sobe um processo **Backend** (ASP.NET Core) em `127.0.0.1` com **porta dinâmica**, guarda a URL em `BackendConfig`, e chama a API via `HttpClient` nomeado `"ApiClient"`. O Backend valida **JWT emitido pelo Amazon Cognito**, executa **Controllers → Services → Repositories** e persiste no **SQLite** local; a **sincronização com a nuvem** usa **Amazon DynamoDB** dentro de `SyncService`.

```
┌─────────────────────────────────────────────────────────────────┐
│  MAUI + Blazor (Frontend)                                        │
│  MauiProgram → BackendStarter → URL em BackendConfig             │
│  HttpClient "ApiClient" + AuthDelegatingHandler                  │
└────────────────────────────┬────────────────────────────────────┘
                             │ HTTP (localhost)
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│  ASP.NET Core (Backend)                                          │
│  JwtBearer (Cognito) → Controllers → Services → Repositories     │
└────────────────────────────┬────────────────────────────────────┘
                             │
              ┌──────────────┴──────────────┐
              ▼                             ▼
       SQLite (local)              DynamoDB (sync)
       canilapp.db                  via IDynamoDBContext
```

---

## 1. Fluxo completo de uma ação do usuário (exemplo: criar produto)

### 1.1 Camada a camada

1. **UI (MAUI + Blazor)**  
   - Rota: `Frontend/Components/Pages/Produtos.razor` (`@page "/produtos"`).  
   - A página injeta `ProdutosViewModel` e mostra abas via `TabsIterator`, `ProdutosCadastroTab.razor` e `ProdutosListarTab.razor`.  
   - O utilizador preenche o formulário ligado a `ProdutosViewModel.ProdutoCadastro` (`Frontend/Models/Produtos/ProdutosModel.cs`).

2. **ViewModel**  
   - `ProdutosViewModel.CadastrarProdutoAsync` (`Frontend/ViewModels/ProdutosViewModel.cs`):  
     - Converte o modelo de UI para DTO com cast explícito: `(ProdutosCadastroDTO)prod` (operador definido em `Frontend/Models/Produtos/ProdutosModel.cs`).  
     - Envia `POST` com `PostAsJsonAsync("api/produtos", dto)` usando o cliente `"ApiClient"` obtido de `IHttpClientFactory`.

3. **HTTP antes de chegar ao Controller**  
   - O `HttpClient` está configurado em `Frontend/MauiProgram.cs` com `AddHttpMessageHandler<AuthDelegatingHandler>()`.  
   - `Frontend/Handlers/AuthDelegatingHandler.cs` adiciona o header `Authorization: Bearer <id_token>` (lido do `SecureStorage`), **exceto** para URLs que contêm `/api/login` (login e refresh não devem levar JWT antigo).

4. **API (Controller)**  
   - `Backend/Controllers/ProdutosController.cs`: classe com `[Authorize]`.  
   - Ação `Create` recebe `[FromBody] ProdutosCadastroDTO dto`.  
   - Converte para entidade de persistência com conversão implícita: `ProdutosModel model = dto` (operador em `Backend/Models/Produtos/ProdutosModel.cs`).  
   - Chama `_service.CriarAsync(model)` — o compilador usa a conversão implícita de `ProdutosModel` para `ProdutosCadastroDTO` onde o serviço espera o DTO (`IProdutosService` / `IService<ProdutosCadastroDTO, ProdutosLeituraDTO>`).

5. **Service**  
   - `Backend/Services/ProdutosService.cs` → `CriarAsync(ProdutosCadastroDTO dto)`: valida campos obrigatórios e pode lançar `ModelIncompletaException` (`Backend/Exceptions/ModelIncompletaException.cs`).  
   - Chama `_repository.CreateAsync(dto)`; o DTO converte-se de novo em `ProdutosModel` para o repositório.

6. **Repository**  
   - `Backend/Repositories/ProdutosRepository.cs` → `CreateAsync(ProdutosModel model)`:  
     - ` _context.Produtos.Add(model)`  
     - `await _context.SaveChangesAsync()`  
   - Contexto: `CanilAppDbContext` (`Backend/Context/CanilAppDbContext.cs`).

7. **Banco (SQLite)**  
   - Configurado em `Backend/Program.cs`: ficheiro `canilapp.db` em `%LocalApplicationData%\CanilApp\canilapp.db`.  
   - O EF Core mapeia `ProdutosModel` (e tipos base como `ItemComEstoqueBaseModel`) para as tabelas definidas em `OnModelCreating` de `CanilAppDbContext`.

### 1.2 Diagrama linear (criar produto)

```
Utilizador
   │
   ▼
Produtos.razor + ProdutosCadastroTab
   │
   ▼
ProdutosViewModel.CadastrarProdutoCommand → CadastrarProdutoAsync
   │
   ▼
HttpClient "ApiClient" + AuthDelegatingHandler (Bearer id_token)
   │
   ▼
ProdutosController.Create [Authorize]
   │
   ▼
ProdutosService.CriarAsync
   │
   ▼
ProdutosRepository.CreateAsync + CanilAppDbContext.SaveChangesAsync
   │
   ▼
SQLite (canilapp.db)
```

O mesmo **padrão** aparece noutras áreas: por exemplo `MedicamentosController` / `MedicamentosService` / `MedicamentosRepository`, `InsumosController` / `InsumosService` / `InsumosRepository`, etc., sempre com `[Authorize]` nos controllers de negócio.

---

## 2. Como o JWT (Cognito) entra no fluxo

### 2.1 Obter tokens (login)

- A UI de login usa `LoginViewModel` (`Frontend/ViewModels/LoginViewModel.cs`).  
- `POST api/login` com corpo anónimo → `Backend/Controllers/LoginController.cs` → `ICognitoService` implementado por `Backend/Services/CognitoService.cs`.  
- `CognitoService.AuthenticateAsync` usa o AWS SDK (`InitiateAuth` / `GetUser`) e devolve `LoginResponseModel` (`Shared/Models/LoginResponseModel.cs`) com `Token` (`Shared/Models/TokenResponse`) e `Usuario`.  
- `LoginViewModel` chama `CustomAuthenticationStateProvider.MarkUserAsAuthenticated` (`Frontend/Services/CustomAuthenticationStateProvider.cs`), que grava no **MAUI `SecureStorage`**, entre outros: `id_token`, `access_token`, `refresh_token`, `auth_token`.

### 2.2 Validar tokens (cada pedido à API protegida)

- Em `Backend/Program.cs` regista-se `AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)`.  
- **Authority** aponta para o User Pool: `https://cognito-idp.{region}.amazonaws.com/{userPoolId}`.  
- Parâmetros vêm de configuração: `AWS:Region`, `AWS:UserPoolId`, `AWS:ClientId` (o `ClientId` é validado na configuração; na validação JWT está `ValidateAudience = false` no código atual).  
- O pipeline usa `app.UseAuthentication()` e `app.UseAuthorization()` antes de `app.MapControllers()`.  
- Controllers como `ProdutosController` têm `[Authorize]`: sem token válido, o pedido não chega à ação.  
- `LoginController` **não** tem `[Authorize]` no nível da classe — login e refresh são públicos na API.

### 2.3 O que o cliente envia como “JWT da API”

- `AuthDelegatingHandler` envia o **ID Token** Cognito como Bearer (`SecureStorage` chave `id_token`), alinhado com o comentário no código: o ID token contém o `aud` esperado para validação típica contra o app client.

### 2.4 Renovação (refresh)

- `CognitoService.RefreshTokenAsync` trata o fluxo no servidor.  
- O cliente pode chamar `POST api/login/refresh` com `RefreshTokenRequest` definido no próprio `LoginController.cs`.  
- `AuthDelegatingHandler.TryRefreshTokenAsync` também chama `api/login/refresh` e atualiza tokens no `SecureStorage` quando consegue ler a resposta como `TokenResponse`.

---

## 3. TenantId — resolução e uso

**No código C# deste repositório não existe propriedade, claim ou serviço chamado `TenantId`.** Não há filtro global por tenant no `CanilAppDbContext` nem nos repositórios.

Documentos como `docs/BLUEPRINT-ARQUITETURA-ALVO-MULTITENANT.md` e `docs/RELATORIO-TECNICO-ARQUITETURA.md` **descrevem uma evolução futura** (ex.: claim `custom:tenantId` no Cognito, coluna `TenantId` no SQLite). Isso **não está implementado** na base de código atual.

Hoje, o isolamento “por cliente” na prática é: **uma instalação** → **um ficheiro SQLite local** → **um User Pool Cognito** configurado por `appsettings`/ambiente.

---

## 4. Comunicação com a API local

### 4.1 Subir o Backend

- `Frontend/MauiProgram.cs` chama `BackendStarter.StartBackendAndGetUrl()` **antes** de construir o `MauiApp`.  
- `Frontend/Services/BackendStarter.cs`:  
  - Procura `backend.json` em `%LocalApplicationData%\CanilApp\backend.json` com `port`, `pid`, `url`.  
  - Se o processo ainda existir e `GET {url}/api/health` responder OK, **reutiliza** essa URL.  
  - Caso contrário, localiza `Backend.exe` ou `Backend.dll`, inicia o processo com `--urls http://127.0.0.1:0` e espera a URL no JSON ou na linha `BACKEND_URL:...` no stdout.

### 4.2 O Backend anuncia-se a si mesmo

- `Backend/Program.cs`, no registo `ApplicationStarted`: escreve `backend.json` com a URL real (porta atribuída pelo Kestrel) e imprime `BACKEND_URL:{httpUrl}` para consola.

### 4.3 O cliente usa essa URL

- `BackendConfig` (`Frontend/Config/BackendConfig.cs`) guarda `Url`.  
- `AddHttpClient("ApiClient", ...)` em `MauiProgram.cs` define `client.BaseAddress = new Uri(cfg.Url)`.

### 4.4 Encerramento

- Ao fechar a app, vários hooks chamam `BackendStarter.ShutdownBackend()`.  
- Primeiro tenta `POST {url}/internal/shutdown` (mapeado em `Backend/Program.cs`); se necessário, faz kill do processo que a app iniciou.

### 4.5 Saúde e CORS

- Health check usado pelo starter: `GET /api/health`.  
- CORS: política `LocalhostOnly` em `Backend/Program.cs` (origens locais / rede privada conforme regra no código).

---

## 5. Sincronização com a nuvem (o que existe hoje)

Existe **sync real implementado** entre **SQLite local** e **DynamoDB**, não apenas documentação.

### 5.1 Disparo a partir da UI

- Em `ProdutosViewModel` (e padrão semelhante em outros view models, ex. insumos), o comando `SincronizarProdutosCommand` faz `POST` para **`api/sync`** (`ProdutosViewModel.SincronizarProdutosAsync`).

### 5.2 API

- `Backend/Controllers/SyncController.cs`: `[Authorize]`, ação `Sincronizar` em `POST api/sync` com `[EnableRateLimiting("sync-policy")]`.  
- Política `sync-policy` definida em `Backend/Program.cs` (`AddRateLimiter`).

### 5.3 Serviço

- `Backend/Services/SyncService.cs` implementa `ISyncService` (`Backend/Services/ISyncService.cs`):  
  - `SincronizarTabelasAsync` chama, em sequência, métodos como `SincronizarMedicamentosAsync`, `SincronizarProdutosAsync`, `SincronizarInsumosAsync`, `SincronizarRetiradaEstoqueAsync`, e por fim `LimparRegistrosExcluidosAsync`.  
  - Usa `IDynamoDBContext` e `CanilAppDbContext`; helpers em `Backend/Helper/SyncHelpers.cs` e lógica partilhada/comentada no próprio `SyncService.cs` (ex.: `GarantirUtcLocal`, `MesclarLotes`).  
- Modelos como `ProdutosModel` têm atributos `[DynamoDBTable("Produtos")]` etc., alinhados com a gravação/leitura no DynamoDB.

### 5.4 Outros endpoints úteis no SyncController

- `POST api/sync/limpar` → `LimparRegistrosExcluidosAsync`.  
- Endpoints de teste/diagnóstico: `GET api/sync/test-dynamo`, `GET api/sync/test-produto-schema` (uso principalmente para desenvolvimento).

### 5.5 Eliminações e sync

- Repositórios como `ProdutosRepository.DeleteAsync` fazem **soft delete** (`IsDeleted = true`, atualiza `DataAtualizacao`), com comentários no código a indicar que isso é **essencial para o sync**.

---

## 6. Outros ficheiros que ajudam a “fechar o puzzle”

| Peça | Ficheiro / classe | Papel |
|------|-------------------|--------|
| Arranque MAUI | `Frontend/MauiProgram.cs` | Backend + DI + HttpClient + auth Blazor |
| Estado auth na UI | `Frontend/Services/AuthenticationStateService.cs`, `CustomAuthenticationStateProvider.cs` | Estado e persistência para componentes autorizados |
| Bypass só de UI (Debug) | `Frontend/Config/DevAuthBypass.cs` | Pode saltar telas; **a API continua a exigir JWT** nas rotas `[Authorize]` |
| Cadastro de utilizador | `CadastroViewModel`, `UsuariosController`, `CognitoService.RegisterUserAsync` | Fluxo separado do CRUD de produtos/medicamentos |
| Base com estoque | `Backend/Models/ItemComEstoqueBaseModel.cs`, `ItemEstoqueModel`, `ItemNivelEstoqueModel` | Partilhado por produtos/insumos/medicamentos |

---

## 7. Resumo rápido

- **UI Blazor** chama **ViewModels** que usam **`HttpClient` "ApiClient"**.  
- **`AuthDelegatingHandler`** junta o **ID token** Cognito às chamadas (menos login/refresh).  
- **Backend** valida JWT com **JwtBearer + Cognito**, depois **Controller → Service → Repository → EF Core → SQLite**.  
- **`TenantId` não está no código**; multi-tenant está só em documentos de arquitetura alvo.  
- **API local**: processo **Backend** + ficheiro **`backend.json`** + **`BackendConfig.Url`**.  
- **Nuvem**: **`SyncService`** sincroniza com **DynamoDB**, disparado por **`POST api/sync`** (com rate limiting).
