# Convenções do projeto CanilApp

Este documento descreve padrões observados **no código atual** da solução (`CanilApp.sln`: projetos **Backend**, **Frontend**, **Shared**). Não há pastas nomeadas *Application*, *Domain* ou *Infrastructure*; a organização segue camadas implícitas no **Backend** (API ASP.NET Core) e um cliente **MAUI Blazor** no **Frontend**, com contratos compartilhados no **Shared**.

---

## 1. Convenções de nomenclatura

### 1.1 Controllers

- **Pasta:** `Backend/Controllers/`
- **Nome da classe:** sufixo `Controller` (ex.: `ProdutosController`, `LoginController`).
- **Rota:** em geral `[Route("api/[controller]")]`, o que expõe URLs como `/api/produtos`, `/api/medicamentos`, etc.
- **Base:** `ControllerBase` com `[ApiController]`.
- **Namespace:** na maioria dos arquivos, `Backend.Controllers`. Há exceções no repositório (por exemplo, um controller sem `namespace` explícito); novos controllers devem seguir o padrão `Backend.Controllers` para consistência com o restante.
- **Autorização:** muitos controllers usam `[Authorize]` na classe; autenticação pública (ex.: `LoginController`, `UsuariosController` para cadastro) não aplica `[Authorize]` no controller.

```csharp
namespace Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProdutosController : ControllerBase
{
    private readonly IProdutosService _service;

    public ProdutosController(IProdutosService service) => _service = service;
    // ...
}
```

### 1.2 Services

- **Implementações:** `Backend/Services/`
- **Interfaces de domínio CRUD:** `Backend/Services/Interfaces/` — nome `I<Nome>Service` com implementação `<Nome>Service`.
- **Interface genérica:** `IService<TCadastroDTO, TLeituraDTO>` em `Backend/Services/Interfaces/IService.cs`, estendida por serviços que seguem o fluxo cadastro/leitura + CRUD assíncrono (`BuscarTodosAsync`, `BuscarPorIdAsync`, `CriarAsync`, `AtualizarAsync`, `DeletarAsync`).
- **Métodos:** nomes em português com sufixo `Async` (ex.: `BuscarTodosAsync`, `CriarAsync`), alinhados ao código existente.
- **Exceções ao “tudo em Interfaces”:** `ISyncService` vive em `Backend/Services/ISyncService.cs`; `ICognitoService` está declarada no mesmo arquivo que `CognitoService` (`Backend/Services/CognitoService.cs`).
- **Injeção sem interface:** alguns serviços são registrados e injetados como classe concreta (ex.: `EstoqueItemService`, `RetiradaEstoqueService` em `Program.cs`). Isso é padrão **existente** no projeto, não uma recomendação nova.

### 1.3 DTOs

- **Pasta:** `Shared/DTOs/`, com subpastas por agregado/domínio (`Produtos/`, `Medicamentos/`, `Insumos/`, `Estoque/`) ou na raiz de `DTOs` para autenticação/usuário (`UsuarioRequestDTO`, `UsuarioResponseDTO`).
- **Nome:** sufixo `DTO`.
- **Variantes usadas:** `*CadastroDTO`, `*LeituraDTO`, `*FiltroDTO`; requisições de API podem usar tipos em `Shared.Models` (ex.: login) conforme cada controller.
- **Namespace:** espelha a pasta, ex.: `Shared.DTOs.Produtos`.

### 1.4 Models (Backend — entidades persistidas / sincronização)

- **Pasta:** `Backend/Models/`, com subpastas por domínio quando aplicável (`Produtos/`, `Insumos/`, `Medicamentos/`, `Usuarios/`) e modelos transversais na raiz de `Models` (ex.: `ItemEstoqueModel`, `RetiradaEstoqueModel`, `ItemComEstoqueBaseModel`).
- **Nome:** sufixo `Model` (ex.: `ProdutosModel`, `MedicamentosModel`).
- **EF Core:** entidades expostas em `CanilAppDbContext` (`Backend/Context/CanilAppDbContext.cs`).
- **DynamoDB:** vários modelos usam atributos `DynamoDBTable` / `DynamoDBProperty` (ex.: `ProdutosModel`), alinhados ao serviço de sincronização.

### 1.5 Models (Frontend — UI / formulários)

- **Pasta:** `Frontend/Models/`, em subpastas por domínio (ex.: `Insumos/InsumosFiltroModel.cs`, `Produtos/ProdutosFiltroModel.cs`).
- **ViewModels:** `Frontend/ViewModels/`, sufixo `ViewModel`, registrados no DI em `Frontend/MauiProgram.cs`.

### 1.6 Repositories

- **Interfaces:** `Backend/Repositories/Interfaces/` — `I<Nome>Repository`, frequentemente derivando de `IRepository<T>` (`Backend/Repositories/Interfaces/IRepository.cs`).
- **Implementações:** `Backend/Repositories/`.
- **Registro:** em `Backend/Program.cs` com `AddScoped<IRepository, Implementação>` (há casos de implementação com nome de classe fora do padrão usual; ao alterar, manter o registro no DI coerente com a interface usada pelo serviço).

---

## 2. Estrutura de pastas e responsabilidades

| Área | Caminho | Responsabilidade |
|------|---------|------------------|
| API HTTP | `Backend/Controllers/` | Endpoints REST, validação de entrada HTTP, códigos de resposta, orquestração fina chamando serviços. |
| Regras / casos de uso | `Backend/Services/` | Lógica de aplicação, composição de repositórios, conversão modelo ↔ DTO quando aplicável. |
| Acesso a dados | `Backend/Repositories/` | Consultas e persistência via `CanilAppDbContext` (EF Core SQLite no projeto). |
| Banco (EF) | `Backend/Context/`, `Backend/Migrations/` | `DbContext`, configuração de entidades, migrações. |
| Entidades | `Backend/Models/` | Mapeamento EF e metadados DynamoDB conforme o modelo. |
| Contratos compartilhados | `Shared/DTOs/`, `Shared/Models/`, `Shared/Enums/`, `Shared/Helpers/`, `Shared/ExtensionMethods/` | DTOs de API, modelos de mensagem comuns, enums, utilitários. |
| Exceções de domínio/API | `Backend/Exceptions/` | Exceções específicas tratadas nos controllers (ex.: `ModelIncompletaException`). |
| Helpers Backend | `Backend/Helper/` | Utilitários do backend (ex.: sincronização). |
| App cliente | `Frontend/` | MAUI + Blazor: `Pages/`, `Components/`, `ViewModels/`, `Models/`, `Services/`, `Handlers/`, `Config/`. |

---

## 3. Onde criar novos artefatos

### 3.1 Novos endpoints

1. **Controller** em `Backend/Controllers/` (ou ação nova em controller existente do mesmo recurso).
2. Rota seguindo o padrão `[Route("api/[controller]")]` e verbos HTTP (`[HttpGet]`, `[HttpPost]`, etc.).
3. Registrar dependências em `Backend/Program.cs` se novos serviços/repositórios forem necessários.
4. Tipos de payload/resposta: preferir DTOs em `Shared/DTOs/...` quando forem contratos da API compartilhados com o Frontend.

### 3.2 Novos serviços

1. Interface (quando seguir o padrão majoritário) em `Backend/Services/Interfaces/I<Nome>Service.cs`.
2. Implementação em `Backend/Services/<Nome>Service.cs`.
3. Registro: `builder.Services.AddScoped<I<Nome>Service, <Nome>Service>();` em `Backend/Program.cs`.
4. Se o serviço seguir CRUD com DTOs de cadastro/leitura, considerar estender `IService<TCadastro, TLeitura>` como em `IProdutosService` / `IMedicamentosService`.

### 3.3 Novas entidades (modelo de dados)

1. Classe de entidade em `Backend/Models/` (subpasta do domínio, se houver agrupamento claro).
2. `DbSet<>` e configuração em `OnModelCreating` em `Backend/Context/CanilAppDbContext.cs`.
3. Nova migração EF em `Backend/Migrations/` (fluxo padrão `dotnet ef migrations add ...` no projeto Backend).
4. Se a entidade participar da sincronização DynamoDB, alinhar atributos e tabelas ao padrão já usado nos modelos existentes (`SyncService` / helpers).

### 3.4 Novos repositórios

1. Interface em `Backend/Repositories/Interfaces/`, possivelmente herdando `IRepository<T>`.
2. Implementação em `Backend/Repositories/`.
3. Registro em `Backend/Program.cs`.

### 3.5 Frontend (telas que consomem a API)

- **ViewModel** novo: `Frontend/ViewModels/` + registro em `MauiProgram.cs` (`AddScoped<...>()`).
- **Modelos de tela** (filtros, estado de formulário): `Frontend/Models/<Domínio>/`.
- **Chamadas HTTP:** uso de `HttpClient` nomeado (`ApiClient` em `MauiProgram.cs`) e, quando necessário, `AuthDelegatingHandler` já registrado no projeto.

---

## 4. Padrões arquiteturais identificados

O Backend **não** está organizado em projetos ou pastas nomeadas *Application* / *Domain* / *Infrastructure*. O que existe na prática é:

1. **Camada de apresentação HTTP:** Controllers.
2. **Camada de aplicação / serviços:** Services (orquestram repositórios e regras).
3. **Camada de persistência:** Repositories + `CanilAppDbContext`.
4. **Integrações externas:** Serviços como `CognitoService`, `SyncService`, uso de AWS SDK (DynamoDB, Cognito) registrados em `Program.cs`.
5. **Contrato compartilhado:** Projeto **Shared** referenciado por Backend e Frontend.

Ou seja: arquitetura em **camadas lógicas** dentro do assembly **Backend**, com **Shared** para DTOs e tipos comuns.

---

## 5. Boas práticas observadas no projeto

### 5.1 Separação de responsabilidades

- Controllers delegam a **serviços**; serviços usam **repositórios** para acesso ao banco.
- DTOs em **Shared** evitam expor diretamente entidades EF em todas as rotas (há conversões implícitas ou mapeamentos entre DTO e `*Model` nos controllers/serviços, conforme cada fluxo).

### 5.2 Injeção de dependências (DI)

- **Backend:** `Program.cs` registra repositórios e serviços com `AddScoped` (e `AddSingleton` onde já usado, ex.: AWS/Cognito).
- **Frontend:** `MauiProgram.cs` registra ViewModels, serviços de autenticação, `HttpClient` factory e handlers.
- Controllers recebem dependências via **construtor** (padrão predominante).

### 5.3 Uso de DTOs

- Entrada/saída de API para recursos principais usa DTOs em `Shared/DTOs` (cadastro, leitura, filtro).
- Respostas de erro padronizadas podem usar `Shared.Models.ErrorResponse` onde já aplicado nos controllers.

### 5.4 Autenticação e autorização

- JWT Bearer (Cognito) configurado em `Program.cs`; controllers protegidos com `[Authorize]` quando o fluxo exige usuário autenticado.

### 5.5 Exceções

- Lançar exceções de negócio específicas (ex.: `ModelIncompletaException`) e tratá-las no controller para retornar status HTTP adequados, como já feito em alguns endpoints.

---

## 6. Referências rápidas no código

- Registro de serviços e repositórios: `Backend/Program.cs` (seção “Services & Repositories”).
- Entidades e `DbSet`: `Backend/Context/CanilAppDbContext.cs`.
- Contrato CRUD genérico de serviço: `Backend/Services/Interfaces/IService.cs`.
- Exemplo de controller + DTO + serviço: `Backend/Controllers/ProdutosController.cs`, `Backend/Services/ProdutosService.cs`, `Shared/DTOs/Produtos/`.
- DI do cliente: `Frontend/MauiProgram.cs`.

Este guia deve ser atualizado quando novas pastas ou convenções forem introduzidas de forma consistente no repositório.
