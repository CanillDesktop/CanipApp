# Relatório Técnico — Arquitetura e Evolução para Multi-Tenant SaaS

**Projeto:** CanilApp — Gerenciamento de estoque (medicamentos e suprimentos)  
**Escopo:** Análise da arquitetura atual e preparação para múltiplos clientes (multi-tenant SaaS)  
**Data:** Fevereiro 2025

---

## 0. Verificação do padrão arquitetural (3-Tier)

### 0.1 Classificação atual

O sistema **pode ser aproximado** a uma **3-Tier Architecture**, porém com fronteiras pouco rígidas e um backend monolítico que concentra Application e Data no mesmo projeto.

| Camada | Onde está hoje | Observação |
|--------|----------------|------------|
| **Presentation** | `Frontend` (MAUI + Blazor WebView) | Cliente desktop; ViewModels e páginas Razor; consumo via `HttpClient` nomeado `"ApiClient"`. |
| **Application / Business** | `Backend` (Controllers + Services) | Lógica de negócio nos Services; Controllers orquestram e expõem API REST. |
| **Data** | `Backend` (Repositories, `CanilAppDbContext`, SQLite, DynamoDB) | Persistência local (EF Core + SQLite) e cloud (DynamoDB); repositórios no mesmo projeto. |

**Conclusão:** Há **separação conceitual** entre apresentação, aplicação e dados, mas **não há separação física** entre Application e Data (ambos no projeto `Backend`). Por isso a classificação formal como 3-Tier é **parcial**.

### 0.2 Separação de responsabilidades

- **Presentation Layer:** Bem delimitada no Frontend (UI, ViewModels, chamadas HTTP). Não acessa banco nem regras pesadas.
- **Application / Business Layer:** Existe (Services + validações), mas:
  - Controllers conhecem DTOs e Models do Backend (ex.: `MedicamentosModel`, `MedicamentoCadastroDTO`).
  - Services recebem/retornam DTOs e às vezes Models; repositórios retornam entidades de persistência.
  - Não há projeto “Application” ou “Domain” separado; regras ficam no Backend.
- **Data Layer:** Repositories + DbContext + SyncService acessam SQLite e DynamoDB diretamente. Models de domínio/persistência vivem em `Backend.Models` e misturam anotações EF Core e DynamoDB na mesma classe (ex.: `MedicamentosModel`).

### 0.3 Pontos onde a separação não está adequada

1. **Models como camada de dados e contrato:** `MedicamentosModel` (e equivalentes) servem ao mesmo tempo de entidade EF, modelo DynamoDB e base para conversão implícita para DTOs. Isso acopla contrato de API, persistência local e cloud.
2. **Controller com lógica desnecessária:** Em `MedicamentosController`, há uso de `IServiceProvider.CreateScope()` para obter `IMedicamentosService` sem uso efetivo (código morto), e conversão DTO → Model no controller (`MedicamentosModel model = medicamentoDto`).
3. **SyncService no Backend:** Contém toda a lógica de sincronização (comparação local vs cloud, merge de lotes, UTC, batch DynamoDB) em um único serviço, misturando orquestração e detalhes de persistência.
4. **Ausência de camada de domínio:** Não existe projeto Domain com entidades “puras” e regras de negócio independentes de EF/DynamoDB.
5. **Shared como “contrato”:** O projeto Shared contém DTOs e enums usados por Frontend e Backend, o que é positivo, mas não há camada de aplicação compartilhada (ex.: interfaces de aplicação).

### 0.4 Ajustes mínimos para classificar formalmente como 3-Tier

1. **Extrair camada de dados para um projeto dedicado (ex.: `Infrastructure` ou `Backend.Data`):** Repositories, DbContext, configuração de SQLite e acesso DynamoDB; manter apenas referência a contratos (interfaces de repositório) em um projeto de aplicação.
2. **Introduzir projeto Application (ou Backend.Application):** Services e interfaces de serviço; Controllers dependem apenas da Application, que por sua vez depende de abstrações de dados (repositórios).
3. **Manter Models de persistência na camada de dados:** Entidades EF/DynamoDB não expostas diretamente nos controllers; usar apenas DTOs na API e mapeamento (manual ou AutoMapper) na Application.
4. **Documentar as três camadas na solution:** Presentation (Frontend), Application (nova), Data/Infrastructure (nova), com dependências unidirecionais: Presentation → Shared; Application → Domain (se criado) + Shared; Data → Application (abstrações).

Com isso, o sistema passa a ter **separação física** das três camadas e pode ser classificado como **3-Tier** de forma clara.

---

## 1. Avaliação da arquitetura atual

### 1.1 Organização de camadas

- **Solution:** 3 projetos — `Frontend`, `Backend`, `Shared`.
- **Frontend:** MAUI (Windows), Blazor WebView; ViewModels (CommunityToolkit.Mvvm); serviços como `BackendStarter` e `AuthDelegatingHandler`; consumo da API via `IHttpClientFactory` com cliente `"ApiClient"`.
- **Backend:** Um único projeto contendo:
  - API (Controllers)
  - Serviços de negócio (Medicamentos, Produtos, Insumos, Usuários, Estoque, Retirada, Sync, Cognito)
  - Repositórios
  - DbContext (EF Core)
  - Models (entidades com anotações EF e DynamoDB)
  - Helpers (SyncHelpers)
- **Shared:** DTOs, Enums, modelos de resposta (ex.: `LoginResponseModel`, `ErrorResponse`), extensões e helpers de exibição.

**Conclusão:** A organização é **por feature/entidade** dentro do Backend (Controller + Service + Repository por domínio), o que é bom para navegação, mas a falta de projetos separados para Application e Data dificulta evolução e testes.

### 1.2 Padrão de persistência de dados

- **Local:** SQLite via EF Core; arquivo em `%LocalApplicationData%\CanilApp\canilapp.db`.
- **Cloud:** AWS DynamoDB; mesmas entidades (ex.: `MedicamentosModel`, `ProdutosModel`) usadas com atributos `[DynamoDBTable]`, `[DynamoDBProperty]`, etc.
- **Padrão:** “Offline-first”: escrita e leitura primárias no SQLite; sincronização bidirecional com DynamoDB sob demanda (endpoint `POST api/sync`).
- **Estratégia de conciliação:** Comparação por `DataAtualizacao`; “last write wins” por registro; para itens com lotes, merge de lotes por data de inserção. Soft delete (`IsDeleted`) para exclusões.
- **Problemas:** Models duplicam responsabilidade (EF + Dynamo + DTO); SyncService faz scan completo das tabelas no DynamoDB (sem filtro por tenant ou partição), o que não escala com múltiplos clientes.

### 1.3 Fluxo de consumo das APIs locais

1. **Inicialização:** `MauiProgram` chama `BackendStarter.StartBackendAndGetUrl()`: inicia o processo Backend (ou reutiliza existente via `backend.json`), obtém porta dinâmica e URL (ex.: `http://127.0.0.1:port`).
2. **Configuração do HTTP:** `BackendConfig` (singleton) guarda a URL; `AddHttpClient("ApiClient", ...)` usa essa URL como `BaseAddress` e adiciona `AuthDelegatingHandler` para injetar o token JWT (Cognito) no header `Authorization`.
3. **Chamadas:** ViewModels usam `IHttpClientFactory.CreateClient("ApiClient")` e fazem `GetFromJsonAsync`, `PostAsJsonAsync`, `DeleteAsync` etc. para rotas como `api/medicamentos`, `api/sync`.
4. **Autenticação:** Login via `api/login` (Cognito); tokens armazenados em `SecureStorage`; validação JWT no Backend com issuer Cognito.

**Conclusão:** O fluxo é claro e adequado para um único cliente local. Para multi-tenant, faltará identificar o tenant (ex.: por token ou por configuração) e propagar esse contexto até a persistência e ao sync.

### 1.4 Integração com AWS para sincronização em nuvem

- **Auth:** AWS Cognito (User Pool) para login; JWT validado no Backend.
- **Armazenamento cloud:** DynamoDB; tabelas por entidade (Medicamentos, Produtos, Insumos, RetiradaEstoque) com modelos compartilhados entre EF e DynamoDB.
- **Sync:** `SyncController` expõe `POST api/sync` (rate-limited); `SyncService.SincronizarTabelasAsync()`:
  - Carrega todos os registros locais (com includes) e todos do DynamoDB (scan sem filtro).
  - Para cada entidade, compara por `DataAtualizacao`, faz merge de lotes quando aplicável, aplica soft delete e envia lotes para o DynamoDB.
  - Chama `LimparRegistrosExcluidosAsync()` para remover do DynamoDB e do SQLite itens marcados como deletados.
- **Sem change tracking incremental:** Cada sync reprocessa todos os dados; não há tabela de “sync metadata” (versões, último sync por tenant).

**Conclusão:** A integração AWS cumpre o objetivo atual de backup/sincronização para um único tenant, mas não está preparada para isolamento por cliente nem para sync incremental.

---

## 2. Análise crítica da estratégia de APIs locais

### 2.1 Vantagens

- **Offline-first real:** Toda operação é feita contra a API local; o app funciona sem internet.
- **Descoberta automática:** Backend na mesma máquina; porta dinâmica e `backend.json` evitam conflito e configuração manual.
- **Stack única:** .NET no cliente e no servidor; DTOs e enums compartilhados reduzem inconsistência.
- **Deploy simples:** Backend é copiado junto ao Frontend (targets no `.csproj` do Frontend); um único instalador para a aplicação desktop.
- **Segurança local:** API escuta em `127.0.0.1`; exposição à rede é limitada (CORS restrito a localhost/LAN).

### 2.2 Desvantagens

- **Um processo Backend por instalação:** Cada máquina roda sua própria API e seu próprio SQLite; não há “servidor central” por organização.
- **Sincronização sob demanda e “full sync”:** Sem fila de mudanças nem particionamento por tenant no DynamoDB; crescimento dos dados aumenta tempo e custo de sync.
- **Duplicação de lógica:** Regras que no futuro precisarem ser garantidas também na nuvem (ex.: validações, limites por plano) terão de ser replicadas em um futuro Cloud API.
- **Escalabilidade de desenvolvimento:** Novos recursos exigem alterações no Backend monolítico; sem camada de aplicação bem separada, refatorações são mais custosas.

### 2.3 Impactos futuros de escalabilidade

- **Multi-tenant:** Hoje não há `TenantId`; todos os dados no DynamoDB e no SQLite são tratados como um único tenant. Incluir múltiplos clientes exigirá:
  - Identificação do tenant (login, token ou configuração).
  - Filtro por tenant em todas as leituras e escritas (local e cloud).
  - Estratégia de particionamento no DynamoDB (partition key com TenantId).
- **Crescimento de dados:** Scan completo no DynamoDB e listas em memória no SyncService não escalam; será necessário sync incremental (change tracking, timestamps, ou fila de eventos).
- **Múltiplas instalações do mesmo cliente:** Várias clínicas/usuários do mesmo “tenant” sincronizando com o mesmo ambiente cloud exigirão resolução de conflitos e possível merge mais sofisticado.

### 2.4 É o melhor caminho para crescimento?

- **Para o cenário atual (um cliente, desktop offline-first):** Sim; a estratégia é coerente e funcional.
- **Para evolução SaaS multi-tenant:** O uso de API local continua válido como **camada de aplicação local** (offline-first), mas deve ser complementado por:
  - **Cloud API** centralizada (multi-tenant), com TenantId e regras de isolamento.
  - **Sync Worker ou serviço de sync** que possa operar em background (local ou em nuvem) com fila de mudanças e retry.
  - **Definição clara:** “fonte da verdade” por dado (local vs cloud) e política de conflitos.

Recomenda-se **evolução gradual**: manter a API local como está no curto prazo, mas ir introduzindo TenantId, camadas e contratos que permitam, no futuro, inserir a Cloud API e o sync orientado a eventos sem reescrita total.

---

## 3. Proposta arquitetural para múltiplos clientes

### 3.1 Introdução de TenantId nas entidades

- Adicionar em todas as entidades de negócio (Medicamentos, Produtos, Insumos, RetiradaEstoque, e na base se houver) uma propriedade **`TenantId`** (string ou Guid).
- No SQLite: coluna `TenantId` em cada tabela; índices compostos nas consultas (ex.: `TenantId + IdItem`).
- No DynamoDB: usar **TenantId como partition key** (ou parte composta da partition key) em todas as tabelas para isolamento e desempenho.
- **Migração:** Para o cliente atual, preencher `TenantId` com um valor fixo (ex.: `"default"` ou um Guid conhecido); novas instalações recebem TenantId no fluxo de onboarding/login.

### 3.2 Modelo inicial de multi-tenant

- **Estratégia recomendada:** **TenantId column (single database)** tanto no SQLite local quanto no DynamoDB.
  - Cada registro pertence a um tenant; todas as queries filtram por `TenantId`.
  - Resolução do tenant: a partir do JWT (claim `custom:tenantId` ou similar no Cognito) ou de tabela usuário → tenant após login.
- **Isolamento:** Garantir que nenhuma query (repositórios, SyncService) retorne ou atualize dados de outro tenant; usar um “TenantContext” ou scoped service que injete o TenantId nas operações.
- **Futuro:** Se um cliente exigir isolamento físico (compliance, performance), evoluir para database-per-tenant (SQLite por tenant) ou schema-per-tenant (quando houver um RDBMS central); no DynamoDB a partition key por TenantId já dá isolamento lógico forte.

### 3.3 Feature flags e parâmetros por cliente

- **Estrutura futura sugerida:**
  - Tabela ou configuração (ex.: DynamoDB ou appsettings por ambiente) `TenantSettings`: `TenantId`, `FeatureFlags` (JSON ou campos), `Limites` (ex.: max itens), `Branding`, etc.
  - Serviço `ITenantSettingsService` (local e depois cloud) que expõe flags e parâmetros; a UI e a aplicação consultam esse serviço para habilitar/desabilitar funcionalidades ou limites.
- **Início:** Pode ser um arquivo JSON ou seção no appsettings por tenant; depois centralizar na nuvem.

### 3.4 Organização dos serviços de sincronização cloud

- **Curto prazo:** Manter `SyncService` no Backend local, mas refatorar para:
  - Receber `TenantId` (do contexto do usuário logado) e filtrar todas as leituras/escritas locais e DynamoDB por esse tenant.
  - Preferir, quando possível, operações DynamoDB por partition key (Query por TenantId) em vez de Scan.
- **Médio prazo:** Introduzir uma **Cloud API** (novo projeto ou host) que:
  - Seja a “fonte da verdade” para dados multi-tenant na nuvem (DynamoDB ou RDBMS).
  - Exponha endpoints de sync (push/pull) por tenant e por entidade, com autenticação e autorização por tenant.
- **Sync Worker (opcional):** Processo em background (local ou em nuvem) que lê uma fila de mudanças (ou change tracking) e envia para a Cloud API; retry e dead-letter para resiliência.

---

## 4. Sugestões práticas de melhorias desde já

Priorizadas por impacto arquitetural, **sem reescrever** o sistema:

1. **Introduzir TenantId nas entidades e no DbContext**
   - Adicionar `TenantId` (string, nullable no início) nas entidades e nas tabelas (migration); no DynamoDB, incluir no modelo e na partition key.
   - Preencher com valor fixo para o cliente atual; todas as queries existentes passam a filtrar por esse TenantId (evita quebra quando houver mais tenants).

2. **Resolução de tenant no pipeline da API**
   - Middleware ou comportamento que, após validação do JWT, leia o tenant (claim ou usuário → tenant) e registre em `IHttpContextAccessor` ou em um `ITenantContext` scoped.
   - Repositórios e SyncService recebem o tenant do contexto em vez de parâmetro solto.

3. **Eliminar código morto e acoplamento nos Controllers**
   - Remover o bloco `using (var scope ... IMedicamentosService)` em `MedicamentosController` (e equivalentes).
   - Mover a conversão DTO → Model para o Service; o Controller só recebe/retorna DTOs.

4. **Padronizar injeção do Sync**
   - Se no futuro o sync for disparado após cada alteração, injetar `ISyncService` e chamar (ou enfileirar) no Service de negócio, não no Controller.

5. **Extrair interfaces de repositório e de serviço em um projeto compartilhado (opcional)**
   - Criar `Shared.Contracts` ou `Backend.Contracts` com interfaces de repositório e de aplicação que o Backend implementa; facilita futura extração da camada de aplicação e testes.

6. **Documentar a arquitetura e o fluxo de sync**
   - Um diagrama (ex.: C4 ou fluxo de dados) e um README em `docs/` descrevendo as camadas e o fluxo local → sync → cloud; ajuda na onboarding e na evolução.

7. **Preparar DynamoDB para tenant**
   - Alterar modelos DynamoDB para incluir `TenantId` como partition key (ou parte da chave composta); criar nova tabela ou migração de dados se necessário; ajustar `SyncService` para usar Query por TenantId em vez de Scan quando possível.

---

## 5. Riscos arquiteturais se o sistema crescer no padrão atual

| Risco | Impacto | Mitigação |
|-------|--------|-----------|
| **Dados de um cliente aparecendo para outro** | Crítico (LGPD/compliance) | Introduzir TenantId e filtrar em todas as camadas; testes de isolamento. |
| **Sync cada vez mais lento e custoso** | Alto (DynamoDB scan, tempo de resposta) | Sync incremental; partition key por TenantId; change tracking. |
| **Regras de negócio duplicadas (local vs cloud)** | Médio (bugs, inconsistência) | Centralizar regras em uma camada de aplicação compartilhada ou na Cloud API. |
| **Backend monolítico difícil de evoluir** | Médio (velocidade de entrega) | Extrair Application e Data para projetos separados; manter APIs estáveis. |
| **Conflitos de concorrência (mesmo tenant, vários dispositivos)** | Médio | Política de conflitos explícita (last-write-wins, ou merge por campo); versionamento ou timestamp. |
| **Falha única no Sync** | Médio | Retry com backoff; fila de mudanças; não bloquear a UI; opção de sync em background. |
| **Configuração e feature flags por cliente** | Baixo no início | Introduzir TenantSettings e serviço de configuração assim que o segundo cliente for planejado. |

---

## Resumo executivo

- A arquitetura atual é **próxima de 3-Tier**, com separação conceitual mas não física entre Application e Data; ajustes mínimos (extrair projetos Application e Data, usar apenas DTOs na API) permitem classificá-la formalmente como 3-Tier.
- A estratégia de **API local** é adequada para o cenário atual (um cliente, offline-first) e pode ser mantida como camada local na evolução; para SaaS multi-tenant, deve ser complementada por **Cloud API**, **TenantId** e **sync orientado a tenant e incremental**.
- **Melhorias prioritárias:** introduzir **TenantId** em todas as entidades e em todas as consultas; **resolver tenant no pipeline** (JWT/context); **refatorar Sync** para ser tenant-aware e, quando possível, incremental; **eliminar código morto** e **melhorar separação** Controller/Service/Repository.
- **Riscos principais:** vazamento de dados entre tenants, crescimento do custo e da latência do sync, e dificuldade de evolução do monolito; todos mitigáveis com as medidas sugeridas de forma gradual.

Este relatório serve de base para o **Blueprint Técnico** (arquitetura-alvo, estrutura da solution, multi-tenant, sync e roadmap) descrito no documento complementar.
