# Documento Técnico – CanilApp (Onboarding de Desenvolvedores)

Este documento descreve o sistema **CanilApp** para desenvolvedores que estão entrando no projeto. O foco é visão geral, arquitetura, estrutura de pastas, fluxos principais, front-end, back-end, padrões identificados, problemas arquiteturais e um guia prático para novos devs.

---

# 1. Visão Geral

## 1.1 Objetivo do software

O **CanilApp** é uma aplicação **desktop para Windows** que funciona como **gestão de estoque e cadastros** voltada ao contexto de canis (medicamentos, produtos, insumos). O aplicativo:

- Roda como um único executável que **inicia o back-end em processo** e abre uma janela com interface Blazor (MAUI + BlazorWebView).
- Permite **login/cadastro de usuários** via **AWS Cognito** (JWT).
- Oferece **CRUD** de Medicamentos, Produtos e Insumos, com **estoque por lotes** (quantidade, data de entrega, validade, NFe).
- **Sincroniza dados** entre um **banco local (SQLite)** e a **nuvem (AWS DynamoDB)** por meio de um fluxo de sync acionado pela UI.

## 1.2 Problema que ele resolve

- Centralizar cadastros e estoque (medicamentos, produtos, insumos) em um único app desktop.
- Permitir uso **offline-first** com banco SQLite local e depois sincronizar com a nuvem (DynamoDB).
- Garantir que apenas usuários autenticados (Cognito) acessem a API e que o app desktop rode de forma autocontida (backend embutido).

## 1.3 Principais módulos

| Módulo | Descrição |
|--------|-----------|
| **Frontend** | App MAUI (Windows) com BlazorWebView; páginas Razor (Login, Cadastro, Home, Medicamentos, Produtos, Insumos, Estoque); ViewModels (MVVM); serviços de autenticação e início do backend. |
| **Backend** | API ASP.NET Core (porta dinâmica); controllers REST (Login, Usuarios, Medicamentos, Produtos, Insumos, Estoque, RetiradaEstoque, Sync); serviços e repositórios; SQLite + DynamoDB. |
| **Shared** | DTOs, enums, modelos comuns (ex.: `ErrorResponse`, `TokenResponse`), helpers e extensões usados por Frontend e Backend. |

## 1.4 Responsabilidades de cada camada

- **Frontend (MAUI + Blazor)**  
  - UI (Razor), navegação, binding com ViewModels, chamadas HTTP à API (com token no header via `AuthDelegatingHandler`), início/parada do processo do backend (`BackendStarter`).
- **Backend (ASP.NET Core)**  
  - Autenticação JWT (Cognito), regras de negócio leves nos Services, acesso a dados nos Repositories, persistência em SQLite e sincronização com DynamoDB no `SyncService`.
- **Shared**  
  - Contratos (DTOs, enums) e tipos comuns para evitar duplicação entre Frontend e Backend.

---

# 2. Arquitetura

## 2.1 Padrão arquitetural

O projeto segue um **padrão em camadas** próximo a **N-tier**:

- **Apresentação**: Frontend (Blazor + ViewModels).
- **API**: Backend (Controllers).
- **Aplicação / Serviços**: Backend (Services com interfaces em `Backend/Services/Interfaces`).
- **Acesso a dados**: Backend (Repositories com interfaces em `Backend/Repositories/Interfaces`), `CanilAppDbContext` (EF Core) e uso direto de `IDynamoDBContext` no `SyncService`.

Não há Clean Architecture explícita: não existe camada de “Domain” com entidades de domínio puras; as “entidades” são os modelos do Backend (anotados para EF e DynamoDB) e os DTOs ficam no Shared. A comunicação entre camadas é:

- **Frontend → Backend**: HTTP (JSON), com `HttpClient` nomeado `"ApiClient"` e `AuthDelegatingHandler` injetado.
- **Backend**: Controller → Service → Repository; Controller → SyncService (e este usa DbContext + DynamoDB).

## 2.2 Como as camadas se comunicam

- **Blazor (Razor)** usa `@inject` para obter ViewModels (e `NavigationManager`, `IJSRuntime` quando necessário). Os eventos da UI disparam comandos do ViewModel (ex.: `ViewModel.CarregarMedicamentosCommand.ExecuteAsync()`).
- **ViewModels** usam `IHttpClientFactory.CreateClient("ApiClient")` para chamar a API; o `AuthDelegatingHandler` adiciona o Bearer token (id_token do Cognito) e trata refresh quando recebe 401.
- **Backend**: injeção de dependência no `Program.cs` (Services e Repositories registrados como Scoped; DynamoDB e Cognito como Singleton/Scoped conforme o caso). Controllers recebem apenas interfaces de Service; Services recebem apenas interfaces de Repository.

## 2.3 Convenções usadas pelos desenvolvedores

- **Backend**
  - Controllers: `[Route("api/[controller]")]`, `[ApiController]`, `[Authorize]` (exceto Login/Cadastro onde aplicável).
  - Serviços: interface `IXxxService` em `Backend/Services/Interfaces`, implementação em `Backend/Services`.
  - Repositórios: interface `IXxxRepository` em `Backend/Repositories/Interfaces`, implementação em `Backend/Repositories`.
  - Nomenclatura: `BuscarTodosAsync`, `BuscarPorIdAsync`, `CriarAsync`, `AtualizarAsync`, `DeletarAsync`; DTOs de cadastro/leitura/filtro no Shared.
- **Frontend**
  - Páginas: `@page "/rota"`, layout por página; uso de `BasePage<TViewModel>` onde há abas (ex.: `Medicamentos.razor`).
  - ViewModels: `ObservableObject` (CommunityToolkit.Mvvm), comandos `IAsyncRelayCommand` / `IRelayCommand`, propriedades com `SetProperty` para notificação.
  - Tabs: componente `TabsIterator` + componentes `XxxListarTab` e `XxxCadastroTab` que recebem o ViewModel por `[Parameter]`.
- **Shared**
  - DTOs em `Shared/DTOs/{Entidade}/` (ex.: `MedicamentoCadastroDTO`, `MedicamentoLeituraDTO`, `MedicamentosFiltroDTO`).
  - Enums em `Shared/Enums`.

## 2.4 Inconsistências arquiteturais

- **Conversão DTO ↔ Model no próprio model**: As conversões entre DTO e entidade estão como operadores implícitos nas classes de modelo do Backend (ex.: `MedicamentosModel` em `Backend/Models/Medicamentos/MedicamentoModel.cs`). Isso acopla o modelo de persistência aos DTOs e dificulta evolução independente.
- **Serviço retornando tipo de leitura mas recebendo cadastro**: Em `MedicamentosController.Post` é usado `MedicamentosModel model = medicamentoDto` e depois `_service.CriarAsync(model)`. A interface do service espera `MedicamentoCadastroDTO`; a conversão implícita Model→DTO no parâmetro mascara o fluxo (Controller trabalha com Model, Service com DTO).
- **Regras de negócio espalhadas**: Validações “obrigatório” aparecem nos Services (ex.: `MedicamentosService.CriarAsync`, `ProdutosService.CriarAsync`) e também em ViewModels (ex.: `CadastroViewModel.RegistrarAsync`). Não há uma camada de domínio ou de aplicação que centralize regras.
- **SyncService muito grande e duplicado**: `SyncService.cs` repete a mesma lógica de sincronização para Medicamentos, Produtos e Insumos (centenas de linhas). Não há abstração genérica por tipo de entidade.
- **Uso de `Debug.WriteLine` e `Console.WriteLine` em produção**: Vários pontos (Controllers, ViewModels, AuthDelegatingHandler) usam isso para “log”; não há uso consistente de `ILogger` no front e no handler.
- **Navegação no Blazor via JS**: Em `LoginPage.razor` e `CadastroPage.razor`, a navegação pós-login/cadastro é feita com `JS.InvokeVoidAsync("eval", "window.location.href = '...'")`, forçando reload completo em vez de navegação SPA.
- **Arquivo de serviço com nome inconsistente**: A implementação do serviço de medicamentos está no arquivo `MedicamentoService.cs` (singular), enquanto a classe se chama `MedicamentosService` (plural) e a interface é `IMedicamentosService`.

---

# 3. Estrutura de Pastas

## 3.1 Visão geral das pastas principais

```
CanipApp/
├── Backend/           # API ASP.NET Core (porta dinâmica, SQLite + DynamoDB)
├── Frontend/          # App MAUI + Blazor (Windows)
├── Shared/            # DTOs, Enums, modelos e helpers compartilhados
└── docs/              # Documentação (este arquivo, blueprints, etc.)
```

## 3.2 Backend

| Pasta/Arquivo | Papel |
|---------------|--------|
| `Controllers/` | Endpoints REST: `LoginController`, `UsuariosController`, `MedicamentosController`, `ProdutosController`, `InsumosController`, `EstoqueController`, `RetiradaEstoqueController`, `SyncController`. |
| `Services/` | Regras de aplicação e orquestração: validação básica, chamada a repositórios, integração Cognito (`CognitoService`), sincronização (`SyncService`). |
| `Services/Interfaces/` | Contratos dos serviços (`IMedicamentosService`, `IProdutosService`, `ISyncService`, etc.). |
| `Repositories/` | Acesso a dados: EF Core (`CanilAppDbContext`), métodos CRUD e filtros. |
| `Repositories/Interfaces/` | Contratos dos repositórios. |
| `Context/` | `CanilAppDbContext` (EF Core, SQLite). |
| `Models/` | Entidades EF Core + anotações DynamoDB: `ItemComEstoqueBaseModel`, `MedicamentosModel`, `ProdutosModel`, `InsumosModel`, `ItemEstoqueModel`, `ItemNivelEstoqueModel`, `RetiradaEstoqueModel`, `UsuariosModel`. |
| `Helper/` | `SyncHelpers` (preparação de entidades para DynamoDB/EF). |
| `Exceptions/` | `ModelIncompletaException` (validação de cadastro). |
| `Migrations/` | Migrations do EF Core. |
| `Program.cs` | Configuração da aplicação, DI, autenticação JWT, CORS, rate limiting, discovery da porta e escrita do arquivo `backend.json`. |

**Onde ficam:**

- **Regras de negócio**: Principalmente nos **Services** (validações simples e orquestração); parte da “regra” está implícita nos **Models** (conversões e soft delete nos repositórios).
- **Acesso a dados**: **Repositories** (EF Core) e **SyncService** (leitura/escrita DynamoDB + leitura/escrita EF).
- **Integrações**: **CognitoService** (AWS Cognito), **SyncService** (DynamoDB); configuração AWS em `appsettings.json` / User Secrets.

## 3.3 Frontend

| Pasta/Arquivo | Papel |
|---------------|--------|
| `Components/` | Componentes Blazor. |
| `Components/Pages/` | Páginas com rota: `LoginPage.razor`, `CadastroPage.razor`, `Home.razor`, `Medicamentos.razor`, `Produtos.razor`, `Insumos.razor`, `EstoqueDetail.razor`, `AddLoteEstoque.razor`, `BasePage.razor`, `Index.razor`. |
| `Components/Tabs/` | Abas de listagem e cadastro: `MedicamentosListarTab.razor`, `MedicamentosCadastroTab.razor`, idem para Produtos e Insumos, mais `TabsIterator.razor`. |
| `Components/Layout/` | `MainLayout.razor`, `LoginLayout.razor`, `NavMenu.razor`, `ThemeToggle.razor`. |
| `Components/SpinnerComponent.razor` | Indicador de carregamento. |
| `Components/Routes.razor` | Roteador Blazor (com `AuthorizeRouteView` e `RedirectToLogin`). |
| `ViewModels/` | ViewModels por tela/fluxo: `LoginViewModel`, `CadastroViewModel`, `MedicamentosViewModel`, `ProdutosViewModel`, `InsumosViewModel`, `EstoqueDetailViewModel`, `AddLoteEstoqueViewModel`. |
| `ViewModels/Interfaces/` | `ILoadableViewModel`, `ITabableViewModel`. |
| `Services/` | `BackendStarter` (iniciar/parar backend e descobrir URL), `AuthenticationStateService`, `CustomAuthenticationStateProvider`. |
| `Handlers/` | `AuthDelegatingHandler` (adicionar Bearer token e refresh em 401). |
| `Models/` | Modelos de tela/filtros (ex.: `MedicamentosModel`, `MedicamentosFiltroModel`) e modelos de estoque; alguns espelham ou complementam DTOs do Shared. |
| `Config/` | `BackendConfig` (URL do backend). |
| `MauiProgram.cs` | Registro de serviços, HttpClient, ViewModels, autenticação, cultura pt-BR e início do backend. |
| `MainPage.xaml` / `MainPage.xaml.cs` | Host do BlazorWebView com `Routes` como root. |

**Onde ficam:**

- **UI / formulários**: `Components/Pages/*.razor` e `Components/Tabs/*.razor`.
- **Regras de apresentação e chamadas à API**: **ViewModels** (comandos e propriedades observáveis).
- **Comunicação com backend**: **ViewModels** via `HttpClient`; **AuthDelegatingHandler** para token; **BackendStarter** para subir/derrubar o processo da API.
- **Models/entidades de tela**: `Frontend/Models/` (podem duplicar ou espelhar DTOs do Shared).

## 3.4 Shared

| Pasta | Conteúdo |
|-------|----------|
| `DTOs/` | DTOs por entidade: Medicamentos, Produtos, Insumos, Estoque, Usuarios (ex.: `MedicamentoCadastroDTO`, `MedicamentoLeituraDTO`, `MedicamentosFiltroDTO`). |
| `Models/` | `ErrorResponse`, `TokenResponse`, `LoginResponseModel`, interfaces como `IEstoqueItem`. |
| `Enums/` | `PrioridadeEnum`, `PublicoAlvoMedicamentoEnum`, `UnidadeEnum`, `CategoriaEnum`, `PermissoesEnum`, etc. |
| `Helpers/` | `DisplayNameHelper`. |
| `ExtensionMethods/` | Extensões para string e enum (ex.: descrição de enum). |

---

# 4. Principais Fluxos

## 4.1 Fluxo de início da aplicação

1. **Ponto de entrada**: `Frontend/MauiProgram.cs` → `CreateMauiApp()`.
2. **Backend**: `BackendStarter.StartBackendAndGetUrl()` (em `Frontend/Services/BackendStarter.cs`):
   - Verifica se já existe `backend.json` em `%LocalAppData%/CanilApp` e se o processo está vivo; se sim, usa essa URL.
   - Caso contrário, localiza `Backend.exe` ou `Backend.dll` em pastas previsíveis (incluindo `Frontend/bin/.../Backend/` após build).
   - Inicia o processo do backend com `--urls http://127.0.0.1:0`.
   - Descobre a porta lendo `backend.json` (escrito no `ApplicationStarted` do backend) ou a linha `BACKEND_URL:` no stdout.
3. **Backend** (`Backend/Program.cs`): Kestrel escuta em `http://127.0.0.1:0`, ao subir grava em `backend.json` a URL, PID e versão.
4. **Frontend**: Registra `BackendConfig` com a URL; registra HttpClient `"ApiClient"` com `BaseAddress` e `AuthDelegatingHandler`; registra ViewModels e autenticação; chama `RegisterKillSwitch()` para encerrar o backend no encerramento do app (ProcessExit, Window.Closed, etc.).

**Arquivos envolvidos**: `MauiProgram.cs`, `BackendStarter.cs`, `Program.cs` (Backend), `BackendConfig.cs`.

## 4.2 Fluxo de login

1. **Entrada**: Usuário acessa `/login` (ou é redirecionado por `RedirectToLogin` quando não autenticado). `LoginPage.razor` exibe o formulário e injeta `LoginViewModel`.
2. **Submit**: Botão “Entrar” chama `ViewModel.LoginCommand.ExecuteAsync(null)`.
3. **LoginViewModel.LoginAsync()**:
   - Validação básica (email e senha preenchidos).
   - `POST api/login` com `{ Login, Senha }` via `HttpClient` (já com `AuthDelegatingHandler`; no primeiro login não há token).
4. **Backend – LoginController.LoginAsync**:
   - Recebe `LoginRequest`, chama `ICognitoService.AuthenticateAsync(login, senha)`.
   - **CognitoService**: integra com AWS Cognito (InitiateAuth); retorna tokens + dados do usuário em `LoginResponseModel`.
5. **Resposta**: Backend retorna 200 com `LoginResponseModel` (Token com IdToken, AccessToken, RefreshToken; Usuario).
6. **LoginViewModel** (após 200):
   - Chama `_authProvider.MarkUserAsAuthenticated(idToken, accessToken, refreshToken, email, nome)`.
   - **CustomAuthenticationStateProvider** / **AuthenticationStateService**: gravam tokens no `SecureStorage` (id_token, access_token, auth_token, refresh_token) e disparam `AuthenticationStateChanged`.
   - Grava em `Preferences`: user_role, user_email, user_name.
   - Dispara `NavigationRequested?.Invoke("/home")`.
7. **LoginPage**: Em `HandleNavigation` chama `JS.InvokeVoidAsync("eval", "window.location.href = '/home'")` (reload completo).

**Arquivos**: `LoginPage.razor`, `LoginViewModel.cs`, `AuthDelegatingHandler.cs`, `LoginController.cs`, `CognitoService.cs`, `CustomAuthenticationStateProvider.cs`, `AuthenticationStateService.cs`.

## 4.3 Fluxo de listagem e filtro de medicamentos

1. **Entrada**: Usuário navega para `/medicamentos`. `Medicamentos.razor` usa `BasePage<MedicamentosViewModel>`, injeta o VM e exibe `MedicamentosListarTab` quando a aba ativa é “Medicamentos”.
2. **Carregamento inicial**: Na página, `VM.OnLoadedAsync()` é acionado (ou botão “Atualizar Dados”); chama `CarregarMedicamentosAsync()`.
3. **MedicamentosViewModel.CarregarMedicamentosAsync()**:
   - Verifica token em `SecureStorage.GetAsync("id_token")` (opcional na VM; o handler já coloca o token).
   - `GET api/medicamentos` com `_http.GetFromJsonAsync<MedicamentoLeituraDTO[]>("api/medicamentos")`.
4. **AuthDelegatingHandler**: Adiciona `Authorization: Bearer <id_token>` e envia a requisição.
5. **Backend – MedicamentosController.Get**:
   - Se não houver query string, chama `_service.BuscarTodosAsync()`; caso contrário, `_service.BuscarTodosAsync(filtro)`.
   - **MedicamentosService**: chama `_repository.GetAsync()` ou `GetAsync(filtro)`.
   - **MedicamentosRepository**: consulta `_context.Medicamentos` com `Include` de estoque e nível, filtro `IsDeleted == false`, retorna `MedicamentosModel`; o service converte para `MedicamentoLeituraDTO` (implícito no model) e retorna.
6. **ViewModel**: Limpa `Medicamentos` e preenche com o resultado; define `MensagemSucesso` ou `MensagemErro`.
7. **Filtro**: Em `MedicamentosListarTab`, o usuário escolhe campo e valor e clica em “Filtrar”. Chama `FiltrarMedicamentosCommand.ExecuteAsync(new PesquisaProduto(ChavePesquisa, ValorPesquisa))` → `BuscarMedicamentosFiltradosAsync` monta `api/medicamentos?{ChavePesquisa}={ValorPesquisa}` e faz GET; a lista é atualizada da mesma forma.

**Arquivos**: `Medicamentos.razor`, `MedicamentosListarTab.razor`, `MedicamentosViewModel.cs`, `MedicamentosController.cs`, `MedicamentoService.cs` (MedicamentosService), `MedicamentosRepository.cs`, `CanilAppDbContext.cs`.

## 4.4 Fluxo de cadastro de medicamento

1. **Entrada**: Na tela de medicamentos, usuário clica em “Cadastrar Novo Medicamento” → `ViewModel.AbreAbaCadastro()` → aba “Cadastrar” com `MedicamentosCadastroTab`.
2. **Formulário**: Binding com `ViewModel.MedicamentoCadastro` (tipo `MedicamentosModel` no front). Ao enviar, o componente chama algo como `CadastrarMedicamentoCommand.ExecuteAsync(ViewModel.MedicamentoCadastro)`.
3. **MedicamentosViewModel.CadastrarMedicamentoAsync(med)**:
   - Converte `med` (MedicamentosModel) para `MedicamentoCadastroDTO` (cast/implícito).
   - `POST api/medicamentos` com `_http.PostAsJsonAsync("api/medicamentos", dto)`.
4. **Backend – MedicamentosController.Post**:
   - Recebe `MedicamentoCadastroDTO`, faz `MedicamentosModel model = medicamentoDto` (implícito) e chama `_service.CriarAsync(model)`. (A assinatura do service é DTO; o compilador converte model→DTO.)
   - **MedicamentosService.CriarAsync(dto)**:
     - Valida campos obrigatórios (cod, nome, fórmula, descrição, prioridade, público, lote); lança `ModelIncompletaException` se faltar algo.
     - Chama `_repository.CreateAsync(dto)`; internamente o repositório recebe um `MedicamentosModel` (conversão implícita DTO→Model).
   - **MedicamentosRepository.CreateAsync**: Adiciona ao `_context.Medicamentos`, `SaveChangesAsync`, retorna o model (com IdItem preenchido).
5. **Controller**: Retorna `CreatedAtAction` com o recurso criado (convertido para DTO na saída).
6. **ViewModel**: Mensagem de sucesso, delay, `CarregarMedicamentosAsync()`, `OnTabChanged`, alert e limpa `MedicamentoCadastro`.

**Arquivos**: `MedicamentosCadastroTab.razor`, `MedicamentosViewModel.cs`, `MedicamentosController.cs`, `MedicamentoService.cs`, `MedicamentosRepository.cs`, `Backend/Models/Medicamentos/MedicamentoModel.cs` (conversões).

## 4.5 Fluxo de exclusão (soft delete) de medicamento

1. **Entrada**: Na listagem, botão “Deletar” em um item chama `DeletarMedicamentoCommand.ExecuteAsync(m)`.
2. **MedicamentosViewModel.DeletarMedicamentoAsync(m)**:
   - Confirma com `DisplayAlert` “Deseja realmente excluir...”.
   - `DELETE api/medicamentos/{m.IdItem}`.
3. **Backend – MedicamentosController.Delete(id)**:
   - `_service.DeletarAsync(id)`.
   - **MedicamentosRepository.DeleteAsync(id)**: Busca o medicamento, seta `IsDeleted = true` e `DataAtualizacao = DateTime.UtcNow`, faz `Update` e `SaveChangesAsync` (soft delete).
4. **ViewModel**: Mensagem de sucesso e novo carregamento da lista.

**Arquivos**: `MedicamentosListarTab.razor`, `MedicamentosViewModel.cs`, `MedicamentosController.cs`, `MedicamentosService`, `MedicamentosRepository.cs`.

## 4.6 Fluxo de sincronização com a nuvem (Sync)

1. **Entrada**: Usuário clica em “Sincronizar na nuvem” (ex.: em `MedicamentosListarTab`). Chama `ViewModel.SincronizarMedicamentoCommand.ExecuteAsync(null)` (ou equivalente em outras telas).
2. **ViewModel**: `POST api/sync` (sem body). O endpoint está protegido por `[Authorize]` e rate limiting `sync-policy`.
3. **Backend – SyncController.Sincronizar()**:
   - Chama `_syncService.SincronizarTabelasAsync()`.
4. **SyncService.SincronizarTabelasAsync()**:
   - Chama em sequência: `SincronizarMedicamentosAsync()`, `SincronizarProdutosAsync()`, `SincronizarInsumosAsync()`, `SincronizarRetiradaEstoqueAsync()`, `LimparRegistrosExcluidosAsync()`.
5. **Lógica típica (ex.: medicamentos)**:
   - Carrega todos os medicamentos locais (EF, com Include de estoque).
   - Faz scan completo da tabela DynamoDB de medicamentos.
   - Para cada item: se existe nos dois, compara `DataAtualizacao`; o mais recente vence; mescla lotes com `MesclarLotes`; aplica correção UTC com `GarantirUtcLocal`.
   - Itens só no local: prepara para DynamoDB (`SyncHelpers.PrepararParaDynamoDB`) e adiciona ao batch de envio.
   - Itens só no Dynamo: prepara para EF e insere no contexto local.
   - Executa batch write no DynamoDB e `SaveChangesAsync` no SQLite.
6. **LimparRegistrosExcluidosAsync**: Remove do DynamoDB e do SQLite os registros com `IsDeleted == true`.
7. **Resposta**: 200 com `{ message = "Sincronização concluída." }`; no front, mensagem de sucesso e recarrega lista.

**Arquivos**: `MedicamentosListarTab.razor` (e similares), `MedicamentosViewModel.cs` (SincronizarMedicamentosAsync), `SyncController.cs`, `SyncService.cs`, `SyncHelpers.cs`, modelos com anotações DynamoDB.

## 4.7 Fluxo de cadastro de usuário

1. **Entrada**: `/cadastro` com `CadastroPage.razor` e `CadastroViewModel`.
2. **Submit**: `RegistrarCommand.ExecuteAsync(null)` → `RegistrarAsync()`.
3. **CadastroViewModel**: Valida nome, email, senha (mín. 8), confirmação de senha e “senha forte” (maiúscula, minúscula, número, especial). Monta `UsuarioRequestDTO` e faz `POST api/usuarios`.
4. **Backend – UsuariosController**: Cria usuário no Cognito (e possivelmente grava em tabela local, conforme implementação do `IUsuariosService`). Retorna sucesso ou erro.
5. **ViewModel**: SuccessMessage e redirecionamento para `/login` via `NavigationRequested`.

**Arquivos**: `CadastroPage.razor`, `CadastroViewModel.cs`, `UsuariosController.cs`, serviços de usuário e Cognito.

---

# 5. Frontend

## 5.1 Organização da UI

- **Rotas**: Definidas por `@page` nas páginas (ex.: `/login`, `/cadastro`, `/home`, `/medicamentos`, `/produtos`, `/insumos`). O roteador está em `Routes.razor` com `AuthorizeRouteView`; rotas não autorizadas mostram `RedirectToLogin`, que redireciona para `/login`.
- **Layouts**: `LoginLayout` para login/cadastro; `MainLayout` para área autenticada (com `NavMenu` e `ThemeToggle`).
- **Páginas com abas**: Medicamentos, Produtos e Insumos usam um ViewModel que implementa `ITabableViewModel`; a página usa `TabsIterator` e exibe ou `XxxListarTab` ou `XxxCadastroTab` conforme `ActiveTab`.
- **Componentes reutilizáveis**: `SpinnerComponent`, `TabsIterator`, cards e botões nas tabs; estilos em arquivos `.razor.css` ou `<style>` no próprio componente.

## 5.2 Como eventos disparam regras

- **Eventos de clique**: `@onclick="() => ViewModel.Comando.ExecuteAsync(...)"` ou `@onclick="ViewModel.Comando.Execute"`. A “regra” está no método do ViewModel associado ao comando (ex.: `CarregarMedicamentosAsync`, `CadastrarMedicamentoAsync`).
- **Binding**: `@bind-Value="ViewModel.Propriedade"` para inputs; alterações atualizam o ViewModel; ao enviar o formulário, o comando usa o estado atual do ViewModel (ex.: `MedicamentoCadastro`).
- **Navegação pós-login/cadastro**: Via evento `NavigationRequested` do ViewModel; a página assina e chama `JS.InvokeVoidAsync("eval", "window.location.href = url")`, forçando reload.

## 5.3 Comunicação com o backend

- **Cliente HTTP**: Registrado em `MauiProgram.cs` como nome `"ApiClient"`, com `BaseAddress = BackendConfig.Url` e pipeline com `AuthDelegatingHandler`.
- **AuthDelegatingHandler**: Lê `id_token` do `SecureStorage`, adiciona header `Authorization: Bearer <token>`; em 401 tenta `POST api/login/refresh` com `refresh_token`, atualiza tokens no SecureStorage e reenvia a requisição.
- **ViewModels**: Recebem `IHttpClientFactory`, fazem `CreateClient("ApiClient")` e chamam `GetFromJsonAsync`, `PostAsJsonAsync`, `DeleteAsync`, etc., usando paths relativos (ex.: `api/medicamentos`, `api/sync`).

## 5.4 Padrões usados (ou ausência)

- **MVVM**: ViewModels com CommunityToolkit.Mvvm (`ObservableObject`, `AsyncRelayCommand`), injeção nos componentes Blazor via `@inject`. Não há binding direto da página para um “code-behind” único; a página é “view” e o ViewModel é o view model.
- **Serviços**: `BackendStarter`, `AuthenticationStateService`, `CustomAuthenticationStateProvider`, `AuthDelegatingHandler`; não existe um “ApiService” ou “Repository” no front; cada ViewModel usa HttpClient diretamente.
- **Ausência de camada de serviços de API no front**: Cada ViewModel conhece URLs e DTOs; qualquer mudança de contrato exige alteração em vários ViewModels.
- **Navegação**: Uso de `NavigationManager` em alguns lugares e de `eval` + `window.location.href` em outros, o que é inconsistente e quebra o modelo SPA.

## 5.5 Problemas de acoplamento

- ViewModels acoplados a DTOs do Shared e a paths da API (strings); não há interface de “cliente de medicamentos/produtos”.
- Componentes de tab recebem o ViewModel inteiro por `[Parameter]` e acessam várias propriedades e comandos; trocar a forma de expor dados exigiria mudar todos os tabs.
- Duplicação de modelos: existem `Frontend/Models/Medicamentos/MedicamentosModel.cs` e DTOs no Shared; em alguns pontos usa-se o DTO, em outros o model do front.
- `Application.Current.MainPage.DisplayAlert` e `SecureStorage` usados diretamente nos ViewModels dificultam testes e reuso.

---

# 6. Backend / Regras de negócio

## 6.1 Onde vivem as regras

- **Validação de cadastro**: Nos **Services** (ex.: `MedicamentosService.CriarAsync`, `ProdutosService.CriarAsync`): checagem de campos obrigatórios e enums; em caso de falha, `ModelIncompletaException`.
- **Soft delete**: Nos **Repositories** (Medicamentos, Produtos, Insumos): em `DeleteAsync` setam `IsDeleted = true` e `DataAtualizacao = DateTime.UtcNow` e fazem Update.
- **Sincronização**: Toda a regra de “quem vence” (data de atualização), mesclagem de lotes e limpeza de excluídos está no **SyncService**; não há domínio explícito, apenas comparação de datas e cópia de propriedades.
- **Autenticação/autorização**: Regras de login e refresh no **CognitoService**; uso de JWT e `[Authorize]` nos controllers.

## 6.2 Separação da UI

- A API não conhece o frontend; a comunicação é apenas HTTP/JSON. Controllers não referenciam tipos do Frontend. A separação está ok nesse sentido; o que falta é separar melhor “aplicação” de “domínio” (hoje a aplicação e o acesso a dados estão misturados com conceitos de “entidade” e “DTO” nas mesmas classes de modelo).

## 6.3 Serviços existentes

- **MedicamentosService**, **ProdutosService**, **InsumosService**: CRUD e listagem com filtro; validação ao criar.
- **UsuariosService**: Cadastro/consulta de usuários (Cognito + possível persistência local).
- **CognitoService**: Login e refresh de tokens (AWS Cognito).
- **SyncService**: Sincronização SQLite ↔ DynamoDB e limpeza de excluídos.
- **EstoqueItemService**, **RetiradaEstoqueService**: Operações de estoque e retirada (uso em controllers de Estoque e RetiradaEstoque).

## 6.4 Domínio implícito

- Não há camada de domínio explícita. “Entidades” são os modelos do Backend (EF + DynamoDB); regras aparecem em Services e Repositories. Conceitos como “item com estoque”, “lote”, “nível mínimo” estão espalhados em `ItemComEstoqueBaseModel`, `ItemEstoqueModel`, `ItemNivelEstoqueModel` e nos DTOs do Shared.

---

# 7. Dependências importantes

## 7.1 Backend

- **ASP.NET Core 8**, **Kestrel**: Host da API.
- **Microsoft.EntityFrameworkCore.Sqlite**: Persistência local.
- **AWSSDK.CognitoIdentity**, **AWSSDK.CognitoIdentityProvider**, **AWSSDK.DynamoDBv2**, **AWSSDK.Extensions.NETCore.Setup**: Autenticação e DynamoDB.
- **Microsoft.AspNetCore.Authentication.JwtBearer**: Validação de JWT (Cognito como issuer).
- **Serilog** (Console + File): Logs.
- **Swashbuckle (Swagger)**: Documentação da API.
- **BCrypt.Net-Next**: Possível uso em senhas (verificar onde é usado).
- **System.Text.Json**: Serialização JSON.
- **Rate limiting**: `Microsoft.AspNetCore.RateLimiting` (política `sync-policy` no Sync).

## 7.2 Frontend

- **.NET MAUI 8** (net8.0-windows10.0.19041.0): Host do app desktop Windows.
- **Microsoft.AspNetCore.Components.WebView.Maui**: Blazor dentro do app.
- **CommunityToolkit.Mvvm**: ViewModels e comandos.
- **Microsoft.Extensions.Http**: `IHttpClientFactory`.
- **Microsoft.AspNetCore.Components.Authorization**: `AuthorizeRouteView`, `AuthenticationStateProvider`.
- **AWSSDK.CognitoIdentityProvider**: Uso eventual no cliente (verificar se só backend usa Cognito diretamente).
- **sqlite-net-pcl**: Possível cache local no front (verificar uso).

## 7.3 Shared

- Apenas tipos e enums; dependências mínimas (ex.: `System.ComponentModel.DataAnnotations` para Display, etc.).

Não há uso de DevExpress ou outros kits de UI pesados; a UI é Blazor com CSS (e Bootstrap-style onde aplicável).

---

# 8. Padrões identificados

- **Injeção de dependência**: Backend e Frontend usam DI; serviços e repositórios registrados no Backend; ViewModels e HttpClient no Frontend.
- **Repository + Service**: Backend segue padrão Repository para acesso a dados e Service para orquestração e validação.
- **DTOs compartilhados**: Shared contém DTOs de cadastro, leitura e filtro; Backend e Frontend referenciam o mesmo projeto Shared.
- **Soft delete**: Exclusão lógica com `IsDeleted` e `DataAtualizacao` para suportar sync.
- **Convenção de nomes**: Async para métodos assíncronos; “Buscar”, “Criar”, “Atualizar”, “Deletar” em português.
- **Operadores implícitos para conversão**: Model ↔ DTO definidos nos próprios models do Backend (acoplamento alto).
- **Discovery do backend**: Arquivo `backend.json` e opcionalmente stdout para a frontend descobrir a URL do backend após subir com porta 0.

---

# 9. Problemas arquiteturais (dívidas técnicas)

- **Acoplamento forte**: Models do Backend conhecem DTOs do Shared e implementam conversões; qualquer mudança em DTO ou modelo afeta o outro. Controllers às vezes convertem DTO→Model antes de chamar o service, e o service recebe DTO (após conversão implícita), o que confunde o fluxo.
- **Código duplicado**: SyncService repete a mesma lógica para Medicamentos, Produtos e Insumos (centenas de linhas); poderia ser genérico por tipo de entidade.
- **Fluxos confusos**: Navegação por `eval` + `window.location.href` em vez de `NavigationManager.NavigateTo`; mistura de responsabilidades em alguns ViewModels (UI alert, SecureStorage, HTTP).
- **Violação de arquitetura**: Regras de validação no ViewModel (cadastro de usuário) e no Service (cadastro de medicamento/produto); não há camada de aplicação/domínio clara. Repositórios recebem DTO por conversão implícita em vez de receber explicitamente o tipo de domínio.
- **Manutenção difícil**: SyncService muito longo; muitos `Console.WriteLine`/`Debug.WriteLine` em vez de ILogger; tratamento de erro em controllers às vezes genérico (StatusCode 500 sem estrutura de erro padronizada em todos os pontos).
- **Nomenclatura**: Arquivo `MedicamentoService.cs` (singular) com classe `MedicamentosService` (plural).
- **Tratamento de 401 no Handler**: O handler lê o corpo da resposta e faz throw de `HttpRequestException` em qualquer não-sucesso; em seguida verifica 401 e tenta refresh. Como a resposta já foi consumida, o retry pode ter limitações dependendo do uso do conteúdo.

---

# 10. Guia para novos desenvolvedores

## 10.1 Rodar o projeto

1. **Pré-requisitos**: .NET 8 SDK; Windows (o app é MAUI Windows); credenciais AWS configuradas (User Secrets ou `appsettings.json`) para Cognito e DynamoDB.
2. **Backend sozinho** (opcional): Abrir solução, definir Backend como projeto de inicialização, rodar; a API sobe com porta fixa ou 0 (ver `Program.cs`). Se for porta 0, a URL é escrita em `%LocalAppData%\CanilApp\backend.json`.
3. **App completo**: Definir **Frontend** como projeto de início e executar. O Frontend compila o Backend (target no `.csproj), copia o Backend para a pasta de saída, inicia o processo do Backend, lê a URL (backend.json ou stdout) e abre a janela com Blazor. Ao fechar o app, o kill switch encerra o processo do Backend.
4. **Rotas úteis**: `/login`, `/cadastro`, `/home`, `/medicamentos`, `/produtos`, `/insumos`. Swagger só está disponível em modo Development (ver `Program.cs`).

## 10.2 Onde alterar ao adicionar um novo campo

- **Medicamento (ex.)**:  
  - **Shared**: `MedicamentoCadastroDTO`, `MedicamentoLeituraDTO`, `MedicamentosFiltroDTO` se for filtro.  
  - **Backend**: `MedicamentosModel` (propriedade + conversões implícitas em `Backend/Models/Medicamentos/MedicamentoModel.cs`), `MedicamentosRepository` (filtro se necessário), `MedicamentosService` (validação se obrigatório).  
  - **Frontend**: `MedicamentosModel` e/ou `MedicamentosFiltroModel` em `Frontend/Models`, `MedicamentosViewModel` e `MedicamentosCadastroTab`/`MedicamentosListarTab` para binding e exibição.  
  - **Sync**: Se o campo for usado na resolução de conflito, ver `SyncService` e `SyncHelpers`.

## 10.3 Onde alterar ao criar um novo CRUD (nova entidade)

- **Shared**: Criar DTOs em `Shared/DTOs/{Entidade}/`.  
- **Backend**: Model em `Backend/Models/` (herdar de base se for item com estoque), DbSet no `CanilAppDbContext`, Migration, Interface + Repository, Interface + Service, Controller. Registrar no `Program.cs`. Se for sincronizado: anotações DynamoDB no model, método em `SyncService` e em `LimparRegistrosExcluidosAsync`, e `SyncHelpers` se necessário.  
- **Frontend**: ViewModel (com HttpClient e comandos), Models em `Frontend/Models`, página `@page "/xxx"`, tabs de listar e cadastrar, registro do ViewModel e rota no menu (`NavMenu.razor`).

## 10.4 Onde ver logs

- **Backend**: Serilog em console e em arquivo em `%LocalAppData%\CanilApp\logs\backend-*.log`.  
- **Frontend**: Saída do console do processo MAUI (e `Debug.WriteLine` em debug). Não há um arquivo de log único no front.

## 10.5 Autenticação

- Login: `POST api/login` com `{ Login, Senha }`. Resposta traz tokens e usuário.  
- Tokens são guardados no **SecureStorage** (id_token, access_token, refresh_token). O **AuthDelegatingHandler** coloca o id_token em todas as requisições; em 401 tenta `POST api/login/refresh` com o refresh_token.  
- Rotas protegidas: Blazor usa `AuthorizeRouteView`; endpoints da API usam `[Authorize]` (exceto login/cadastro onde fizer sentido).

---

# 11. Sugestões de melhoria imediata

1. **Centralizar validação**: Criar uma camada de “Application” ou validadores (FluentValidation ou DataAnnotations) e usar tanto no Backend quanto, quando fizer sentido, no Frontend, em vez de duplicar regras em ViewModel e Service.
2. **Remover navegação por `eval`**: Usar `NavigationManager.NavigateTo(url)` e garantir que o estado de autenticação seja atualizado antes (evento do AuthenticationStateProvider já faz isso); assim a navegação permanece SPA.
3. **Abstrair chamadas à API no Frontend**: Criar serviços como `IMedicamentosApiService` que encapsulem `GET/POST/PUT/DELETE api/medicamentos`; ViewModels dependem da interface, facilitando testes e mudanças de contrato.
4. **Refatorar SyncService**: Extrair um “SyncStrategy” ou método genérico por tipo (ItemComEstoqueBaseModel) para evitar triplicação da lógica de medicamentos/produtos/insumos.
5. **Padronizar erros da API**: Retornar sempre um DTO (ex.: `ErrorResponse`) em erro (400, 404, 500) e usar `ILogger` em vez de `Debug.WriteLine`/`Console.WriteLine` no Backend e no Handler.
6. **Renomear arquivo**: `MedicamentoService.cs` → `MedicamentosService.cs` para alinhar com o nome da classe e da interface.
7. **Mapeamento explícito**: Trocar operadores implícitos nos models por mapeamentos explícitos (extension methods ou AutoMapper em um projeto de aplicação), para desacoplar DTOs das entidades de persistência.
8. **Testes**: Adicionar testes unitários para Services (validação e fluxo) e testes de integração para endpoints críticos (login, CRUD, sync), mesmo que em número pequeno no início.

---

*Documento gerado com base na análise do repositório (Backend, Frontend e Shared) e destinado a onboarding de novos desenvolvedores. Recomenda-se manter este arquivo atualizado quando a arquitetura ou os fluxos principais mudarem.*
