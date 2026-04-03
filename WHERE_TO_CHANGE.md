# Onde alterar código (CanilApp / CanipApp)

Guia prático para localizar mudanças. A solução é **.NET 8**: API ASP.NET Core em `Backend/`, cliente **MAUI + Blazor WebView** em `Frontend/`, contratos compartilhados em `Shared/`. O frontend embute e inicia o backend localmente (veja `Frontend/MauiProgram.cs` e `Frontend/Services/BackendStarter.cs`).

---

## Criar endpoint

### Onde mexer

| Camada | Pasta / arquivo |
|--------|------------------|
| Rota HTTP | `Backend/Controllers/` — novo `*Controller.cs` ou ação em controller existente (`MedicamentosController`, `ProdutosController`, `InsumosController`, `EstoqueController`, `RetiradaEstoqueController`, `SyncController`, `LoginController`, `UsuariosController`) |
| Contrato API (entrada/saída) | `Shared/DTOs/` (e às vezes `Shared/Models/`) |
| Injeção de dependência | `Backend/Program.cs` — `builder.Services.AddScoped<...>()` para novos serviços/repositórios |
| Endpoints mínimos (sem controller) | `Backend/Program.cs` — `app.MapGet` / `app.MapPost` (ex.: `/api/health`) |

Controllers usam `[Route("api/[controller]")]` e, na maioria dos casos, `[Authorize]` (exceto login e criação de usuário conforme o controller).

### Fluxo de impacto

1. Defina DTOs em `Shared/` se o contrato for compartilhado com o Blazor.
2. Implemente lógica no **service** (`Backend/Services/`) e persistência no **repository** (`Backend/Repositories/`) quando houver banco ou integração externa.
3. Exponha a ação no controller (validação básica, status HTTP, chamada ao service).
4. Registre interfaces/implementações em `Program.cs`.
5. No **frontend**, o cliente HTTP nomeado `ApiClient` (`Frontend/MauiProgram.cs`) aponta para a URL do backend; chame o novo path a partir do **ViewModel** correspondente (`Frontend/ViewModels/`) com `HttpClient` injetado.
6. Se precisar de tela nova, adicione rota Blazor em `Frontend/Components/Pages/*.razor` (`@page "/..."`) e link em `Frontend/Components/Layout/NavMenu.razor` se for item de menu.

---

## Alterar regra de negócio

### Onde mexer

| Tipo de regra | Local principal |
|---------------|-----------------|
| Validação e orquestração de caso de uso | `Backend/Services/*.cs` e `Backend/Services/Interfaces/` |
| Regras ao ler/gravar dados | `Backend/Repositories/*.cs` e `Backend/Repositories/Interfaces/` |
| Regras de domínio ligadas ao modelo | `Backend/Models/` (hierarquia de itens/estoque, medicamentos, produtos, insumos, etc.) |
| Erros de negócio explícitos | `Backend/Exceptions/` (ex.: `ModelIncompletaException`) |
| Sincronização / nuvem | `Backend/Services/SyncService.cs`, `Backend/Helper/SyncHelpers.cs`, `Backend/Controllers/SyncController.cs` |

Controllers devem permanecer finos: idealmente só delegam ao service.

### Fluxo de impacto

1. Alteração no service pode exigir ajuste no repository e nos **DTOs** (`Shared/DTOs/`) se a resposta ou payload mudar.
2. Se a API mudar, atualize os **ViewModels** do frontend que fazem `GetFromJsonAsync` / `PostAsJsonAsync` e os **models** em `Frontend/Models/` quando espelharem o contrato.
3. Se a regra afetar o que o usuário vê ou valida antes de enviar, alinhe **componentes Razor** em `Frontend/Components/` (tabs, páginas) e possivelmente **attributes** em `Frontend/Attributes/`.

---

## Alterar banco

### Onde mexer

| Aspecto | Local |
|---------|--------|
| SQLite (app local) — contexto EF | `Backend/Context/CanilAppDbContext.cs` |
| Entidades / mapeamento | `Backend/Models/`, configuração em `OnModelCreating` no `CanilAppDbContext` |
| Migrations EF Core | `Backend/Migrations/` |
| String de conexão / arquivo `.db` | `Backend/Program.cs` (path em `%LocalAppData%\CanilApp\canilapp.db`) |
| DynamoDB / sync | Configuração AWS em `Backend/Program.cs` + serviços AWS; modelos/consultas em `SyncService` e repositórios relacionados |

**Observação:** no `Program.cs` há trecho de `Migrate()` comentado; em ambiente de desenvolvimento, confirme como a equipe aplica migrations antes de depender disso em produção.

### Fluxo de impacto

1. Ajustar entidade → atualizar `DbContext` (novos `DbSet`, relacionamentos, conversões).
2. Gerar/aplicar migration (`Backend` como projeto de startup) — arquivos novos em `Backend/Migrations/`.
3. Ajustar **repositories** e **services** que projetam ou persistem essas entidades.
4. Expor ou alterar **DTOs** em `Shared/` e endpoints nos **controllers**.
5. Frontend: ViewModels e modelos que refletem os campos novos ou alterados.

---

## Alterar UI

### Onde mexer

| Área | Pasta / arquivo |
|------|------------------|
| Páginas (rotas Blazor) | `Frontend/Components/Pages/*.razor` (`@page`, layout, fluxo) |
| Abas reutilizáveis por domínio | `Frontend/Components/Tabs/*.razor` |
| Layout geral, menu, login shell | `Frontend/Components/Layout/` (`MainLayout.razor`, `NavMenu.razor`, `LoginLayout.razor`, `ThemeToggle.razor`) |
| Roteamento e autorização de rotas | `Frontend/Components/Routes.razor` |
| Estilos globais / tema | `Frontend/wwwroot/css/app.css`, `Frontend/wwwroot/css/theme.css`, Bootstrap em `wwwroot/css/bootstrap/` |
| Shell MAUI (host WebView) | `Frontend/MainPage.xaml`, `Frontend/App.xaml` |
| Estado e chamadas HTTP | `Frontend/ViewModels/` |

### Fluxo de impacto

1. Página injeta **ViewModel** (`@inject` + muitas páginas herdam `BasePage<TViewModel>` em `BasePage.razor`).
2. Mudanças visuais em `.razor` podem exigir propriedades/comandos novos no ViewModel.
3. Estilos compartilhados vão para `wwwroot/css/`; o `index.html` carrega Bootstrap, `app.css`, `theme.css` e `Frontend.styles.css`.

---

## Alterar autenticação

### Onde mexer

| Camada | Local |
|--------|--------|
| Login / refresh na API | `Backend/Controllers/LoginController.cs` |
| Integração Cognito (tokens, registro) | `Backend/Services/CognitoService.cs` (interface em `ICognitoService` no mesmo projeto) |
| Validação JWT nas APIs | `Backend/Program.cs` — `AddAuthentication(JwtBearerDefaults.AuthenticationScheme)` com Authority do User Pool |
| Criação de usuário (Cognito) | `Backend/Controllers/UsuariosController.cs` |
| Config AWS (região, pool, client) | `Backend/appsettings.json` / `appsettings.*.json` — chaves como `AWS:Region`, `UserPoolId`, `ClientId` |
| Cliente: armazenamento de token e estado | `Frontend/Services/AuthenticationStateService.cs`, `Frontend/Services/CustomAuthenticationStateProvider.cs` |
| Cliente: anexar Bearer e refresh | `Frontend/Handlers/AuthDelegatingHandler.cs` |
| Registro DI Blazor auth | `Frontend/MauiProgram.cs` — `AddAuthorizationCore`, `AuthenticationStateProvider`, `AuthDelegatingHandler` |
| Rotas protegidas | `Frontend/Components/Routes.razor` (`AuthorizeRouteView`) |
| Bypass só dev (se usado) | `Frontend/Config/DevAuthBypass.cs` |

### Fluxo de impacto

1. Mudança no Cognito ou nos claims → ajustar `Program.cs` (validação JWT) e possivelmente `CognitoService`.
2. Mudança no fluxo de login/refresh → `LoginController` + `AuthDelegatingHandler` + `AuthenticationStateService` devem permanecer coerentes (paths `/api/login`, `/api/login/refresh` são tratados no handler).
3. Novas políticas de autorização no backend → marcar controllers/ações com `[Authorize]` e, no frontend, `[Authorize]` em páginas ou `<AuthorizeView>` onde aplicável.

---

## Adicionar abas no front

### Onde mexer

| Passo | Arquivo / padrão |
|-------|-------------------|
| Modelo de aba | `Frontend/Models/TabItemModel.cs` |
| Contrato do ViewModel com abas | `Frontend/ViewModels/Interfaces/ITabableViewModel.cs` |
| Lista de abas e `ActiveTab` | ViewModel do domínio (ex.: `Frontend/ViewModels/ProdutosViewModel.cs` — `TabsShowing`, `HasTabs`, `ActiveTab`) |
| Renderização das abas | `Frontend/Components/Tabs/TabsIterator.razor` |
| Conteúdo por aba | Novo `.razor` em `Frontend/Components/Tabs/` + condicional na página (ex.: `Frontend/Components/Pages/Produtos.razor` alterna `ProdutosCadastroTab` / `ProdutosListarTab` conforme `VM.ActiveTab`) |
| Padrão de outras entidades | `Medicamentos.razor`, `Insumos.razor` + respectivos `*ListarTab` / `*CadastroTab` |

### Fluxo de impacto

1. ViewModel implementa `ITabableViewModel`, preenche `ObservableCollection<TabItemModel>` e define `HasTabs = true` quando aplicável.
2. Na página, use `<TabsIterator ViewModel="VM" />` e `@if (VM.ActiveTab == "...")` para trocar o componente filho.
3. Conecte `VM.OnTabChanged = StateHasChanged` no `OnInitialized` da página (padrão já usado em `Produtos.razor` e similares) para redesenhar ao clicar na aba.

---

## Adicionar ícones

### Onde mexer

| Uso | Como o projeto faz hoje |
|-----|-------------------------|
| Ícones na UI Blazor (botões, menu, títulos) | Classes **Bootstrap Icons** (`bi bi-*`) em `.razor`; CDN em `Frontend/wwwroot/index.html` (`bootstrap-icons`) |
| Ícones / imagens do app MAUI | `Frontend/Resources/Images/` (incluído via `Frontend.csproj` — `MauiImage`) |
| Ícone e splash do pacote | `Frontend/Resources/AppIcon/`, `Frontend/Resources/Splash/`, assets Windows em `Frontend/Platforms/Windows/Assets/` |

### Fluxo de impacto

1. **Só interface Blazor:** escolha classe em [Bootstrap Icons](https://icons.getbootstrap.com/) e use `<i class="bi bi-nome"></i>` ou `<span class="bi ...">` como em `NavMenu.razor`.
2. **Novo ícone como recurso de imagem:** adicione arquivo em `Resources/Images/`, referencie no XAML ou em CSS/HTML conforme o caso (MAUI gera nomes de recurso a partir do arquivo).
3. Alterar CDN ou versão de Bootstrap Icons → `wwwroot/index.html` (impacta todas as telas que usam `bi`).

---

## Mapa rápido de pastas

| Projeto | Responsabilidade |
|---------|------------------|
| `Backend/` | API, EF Core, Cognito, DynamoDB/sync, migrations |
| `Frontend/` | MAUI host, Blazor UI, ViewModels, HttpClient + auth handler |
| `Shared/` | DTOs, enums, modelos compartilhados com o cliente |

Arquivo de solução: `CanilApp.sln` na raiz do repositório.
