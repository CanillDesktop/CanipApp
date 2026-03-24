# Blueprint Técnico — Arquitetura-Alvo Multi-Tenant SaaS

**Projeto:** CanilApp — Controle de medicamentos e suprimentos  
**Objetivo:** Arquitetura-alvo preparada para evolução multi-tenant SaaS, mantendo compatibilidade com operação atual (offline-first + sincronização cloud).  
**Abordagem:** Evolução gradual, sem reescrita total.

---

## 1. Arquitetura lógica recomendada

### 1.1 Modelo arquitetural: 3-Tier híbrido local + cloud

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          PRESENTATION LAYER                                   │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │  Client.Desktop (MAUI + Blazor)                                        │  │
│  │  - UI, ViewModels, Auth (Cognito tokens)                                │  │
│  │  - Consome: Local Backend API (primário) / Cloud API (sync, config)    │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
                    │                                    │
                    ▼                                    ▼
┌───────────────────────────────────┐    ┌───────────────────────────────────┐
│   APPLICATION LAYER (LOCAL)        │    │   APPLICATION LAYER (CLOUD)       │
│   Backend.LocalApi                │    │   Backend.CloudApi                 │
│   - Controllers REST              │    │   - Controllers REST (sync, admin) │
│   - Services (negócio)            │    │   - Serviços multi-tenant          │
│   - TenantContext (do JWT)        │    │   - TenantContext (do JWT)        │
│   - Chama SyncWorker ou sync      │    │   - Fonte da verdade cloud         │
└───────────────────────────────────┘    └───────────────────────────────────┘
                    │                                    │
                    ▼                                    ▼
┌───────────────────────────────────┐    ┌───────────────────────────────────┐
│   DATA LAYER (LOCAL)               │    │   DATA LAYER (CLOUD)               │
│   - SQLite (EF Core)               │    │   - DynamoDB (ou RDBMS futuro)     │
│   - Arquivo por instalação        │    │   - Partition por TenantId         │
│   - Filtro por TenantId           │    │   - Isolamento por tenant          │
└───────────────────────────────────┘    └───────────────────────────────────┘
                    │                                    │
                    └──────────────┬─────────────────────┘
                                   ▼
                    ┌───────────────────────────────────┐
                    │   Backend.SyncWorker (opcional)    │
                    │   - Change tracking / fila         │
                    │   - Push/Pull por tenant           │
                    │   - Retry e conflitos              │
                    └───────────────────────────────────┘
```

- **Client (Presentation):** Aplicação desktop; usa prioritariamente a **Local API** para todas as operações (offline-first); opcionalmente chama **Cloud API** para sync, configuração por tenant ou recursos futuros centralizados.
- **Local Backend API (Application Layer):** Responsável por autenticação (JWT Cognito), resolução de tenant, regras de negócio e persistência local; orquestra a sincronização (chamando SyncWorker ou fazendo push para a Cloud API).
- **Cloud API (Application Layer Cloud):** Fonte da verdade na nuvem para multi-tenant; expõe endpoints de sync (push/pull), configuração por tenant e, no futuro, relatórios ou integrações; sempre com TenantId no contexto.
- **Data Layer local:** SQLite por instalação; todas as tabelas com TenantId; repositórios e DbContext filtram por tenant.
- **Data Layer cloud:** DynamoDB (atual) com partition key por TenantId; evolução futura pode incluir RDBMS (schema-per-tenant ou database-per-tenant) se necessário.

### 1.2 Fluxo completo de dados

**Operação local (leitura/escrita):**

1. Usuário interage com a UI → ViewModel chama `HttpClient` (BaseAddress = URL do Backend local).
2. `AuthDelegatingHandler` adiciona JWT ao request.
3. Local API valida JWT, extrai tenant (claim ou usuário → tenant), preenche `ITenantContext`.
4. Controller chama Service; Service chama Repository com tenant implícito (via contexto).
5. Repository grava/ lê no SQLite com `Where(x => x.TenantId == tenantContext.TenantId)`.
6. Resposta retorna ao cliente em DTO.

**Persistência local:** Sempre em SQLite, com TenantId; uma instalação pode ter um único tenant (configuração) ou, no futuro, troca de contexto (multi-tenant no mesmo app).

**Sincronização cloud:**

1. Cliente dispara sync (botão ou política) → `POST api/sync` na Local API (com JWT).
2. Local API valida JWT, resolve tenant, chama SyncService (ou enfileira para SyncWorker).
3. SyncService: lê do SQLite apenas registros do tenant; compara com DynamoDB (Query por TenantId); aplica regras de conflito; envia lotes para DynamoDB e atualiza local com dados da nuvem quando a nuvem “vence”.
4. Opcional: SyncWorker envia mudanças para a Cloud API em vez de escrever direto no DynamoDB; Cloud API persiste no DynamoDB e pode aplicar regras adicionais.

**Retorno de dados sincronizados:** Após o sync, o cliente pode recarregar listas (ex.: `GET api/medicamentos`) que já refletem os dados mesclados no SQLite local.

---

## 2. Estrutura recomendada da Solution (.NET)

### 2.1 Projetos propostos

| Projeto | Responsabilidade | Dependências |
|--------|-------------------|--------------|
| **Client.Desktop** | Frontend MAUI + Blazor; ViewModels; consumo Local API (e opcionalmente Cloud API); BackendStarter, Auth. | Shared, Shared.Contracts (opcional) |
| **Backend.LocalApi** | API REST local; Controllers; Services de negócio; TenantContext; injeção de Repositories e Sync. | Application, Infrastructure (ou interfaces em Application), Shared |
| **Backend.SyncWorker** | Processo ou biblioteca que executa sync (change tracking, push/pull); pode rodar in-process na LocalApi ou como worker separado. | Application, Infrastructure, Shared |
| **Backend.CloudApi** | API REST na nuvem; multi-tenant; endpoints de sync, configuração, admin. | Application, Infrastructure, Shared |
| **Domain** | Entidades de domínio “puras” (sem anotações EF/DynamoDB); interfaces de repositório de domínio; eventos de domínio (opcional). | Nenhum (ou apenas Shared para enums) |
| **Application** | Casos de uso (services de aplicação); interfaces de repositório e de serviços; DTOs de entrada/saída; validações; uso do Domain. | Domain, Shared |
| **Infrastructure** | Implementação de repositórios; DbContext (EF Core); acesso DynamoDB; implementação de Sync (leitura/escrita local e cloud). | Application, Domain, Shared |
| **Shared** | DTOs comuns, Enums, modelos de resposta, contratos de API (ex.: sync request/response). | Nenhum |
| **Shared.Contracts** (opcional) | Interfaces de serviços e repositórios expostas para o Client (para tipagem forte ou geração de cliente). | Shared |

### 2.2 Diagrama de dependências (alvo)

```
Client.Desktop     →  Shared
Backend.LocalApi   →  Application, Infrastructure, Shared
Backend.SyncWorker →  Application, Infrastructure, Shared
Backend.CloudApi   →  Application, Infrastructure, Shared
Application        →  Domain, Shared
Infrastructure     →  Application, Domain, Shared
Domain            →  (nenhum ou Shared)
Shared            →  (nenhum)
```

- **Application** não referencia Infrastructure: as implementações de repositório são registradas no host (LocalApi, CloudApi, SyncWorker) que referencia Application + Infrastructure.
- **Domain** contém apenas entidades e, se desejar, interfaces de repositório em termos de domínio; sem EF nem DynamoDB.

### 2.3 Responsabilidade resumida por projeto

- **Client.Desktop:** Apresentação; inicia Backend local; chama APIs; não acessa banco.
- **Backend.LocalApi:** Ponto de entrada da aplicação desktop; autenticação; resolução de tenant; orquestração de negócio e sync.
- **Backend.SyncWorker:** Execução da sincronização (in-process ou separada); change tracking; retry; pode chamar Cloud API ou DynamoDB diretamente conforme estratégia.
- **Backend.CloudApi:** Serviço centralizado na nuvem; multi-tenant; sync e configuração.
- **Domain:** Regras e entidades de domínio; sem detalhes de persistência.
- **Application:** Orquestração de casos de uso; interfaces de repositório/serviços; validações.
- **Infrastructure:** Persistência (EF, DynamoDB); implementação de repositórios e de sync.
- **Shared:** Contratos e tipos compartilhados entre cliente e servidores.

---

## 3. Modelo inicial de Multi-Tenant

### 3.1 Estratégia: TenantId column (single database)

- **Local (SQLite):** Uma única base por instalação; cada tabela tem coluna `TenantId` (string ou Guid); todas as queries filtram por `TenantId`.
- **Cloud (DynamoDB):** Partition key = `TenantId` (ou PK composta `TenantId#EntityId`); evita Scan global e garante isolamento por tenant.

### 3.2 Entidades base com TenantId

- Criar interface (ex.: em Domain ou Shared):

```csharp
public interface ITenantEntity
{
    string TenantId { get; set; }
}
```

- Todas as entidades de negócio (Medicamentos, Produtos, Insumos, RetiradaEstoque, etc.) implementam `ITenantEntity` e possuem propriedade `TenantId`.
- No EF Core: configurar globalmente com filtro de query por tenant (global query filter) usando o valor do `ITenantContext` no momento da consulta.
- No DynamoDB: `TenantId` como partition key em todas as tabelas; ao salvar/carregar, sempre usar o tenant do contexto.

### 3.3 Isolamento futuro (database-per-tenant ou schema-per-tenant)

- **Curto/médio prazo:** Manter single database (SQLite local, DynamoDB na nuvem) com TenantId; suficiente para a maioria dos casos SaaS.
- **Se necessário (compliance, performance):**
  - **Database-per-tenant:** Um arquivo SQLite por tenant por instalação, ou um banco por tenant na nuvem; resolução de connection string por tenant.
  - **Schema-per-tenant:** Em um RDBMS central (ex.: PostgreSQL), um schema por tenant; mesma aplicação, connection string ou catalog por tenant.
- A introdução de TenantId desde já permite evoluir para essas estratégias sem mudar o modelo de domínio, apenas a camada de persistência.

### 3.4 Resolução do tenant no login

- **Cognito:** Incluir no User Pool um atributo customizado (ex.: `custom:tenantId`) preenchido no cadastro ou no pré-login (escolha de organização).
- **Fluxo:** Após login, o Backend valida o JWT e lê o claim `tenantId` (ou busca usuário → tenant em tabela local/nuvem); define o tenant no request (middleware ou service scoped).
- **Fallback para cliente único:** Se não houver claim, usar tenant default (ex.: `"default"`) para compatibilidade com o cenário atual.

---

## 4. Arquitetura de sincronização

### 4.1 Modelo recomendado: change tracking + sync por tenant

- **Change tracking:** Manter uma tabela local (ex.: `SyncOutbox` ou `PendingChanges`) com: `Id`, `TenantId`, `EntityType`, `EntityId`, `Operation` (Insert/Update/Delete), `Payload` (opcional), `CreatedAt`, `SyncedAt` (null = pendente).
- **Fluxo de escrita:** Ao criar/atualizar/deletar (soft delete), além de persistir na tabela de negócio, inserir registro na `SyncOutbox` para o tenant atual.
- **Sync:** SyncWorker (ou SyncService) lê registros pendentes da Outbox (por tenant), envia para a Cloud API ou para o DynamoDB em lotes, e marca `SyncedAt`; na direção inversa (pull), Cloud API ou DynamoDB retorna alterações desde `LastSyncedAt` do tenant e aplica no SQLite local.
- **Vantagem:** Evita scan completo; só transmite mudanças; permite retry por registro.

### 4.2 Tabelas de controle sugeridas

- **SyncOutbox (local):** Ver acima.
- **SyncMetadata (local ou cloud):** Por tenant: `TenantId`, `EntityType`, `LastSyncedAt` (UTC), `LastSequence` (opcional), para pull incremental.
- **No DynamoDB:** Manter `DataAtualizacao` (ou versão) por registro para resolução de conflitos e para pull por “alterados desde X”.

### 4.3 Conflitos

- **Estratégia inicial:** Last-write-wins por `DataAtualizacao` (ou por versão), como hoje; para lotes, merge por lote (como no SyncService atual).
- **Futuro:** Opcionalmente, version (número ou timestamp) por entidade; em conflito, retornar 409 e deixar o cliente decidir ou aplicar regra de negócio (ex.: soma de quantidades).

### 4.4 Retry

- Sync em lotes; em falha de rede ou 5xx, reenfileirar e retry com backoff exponencial (ex.: 1s, 2s, 4s); após N tentativas, marcar como “failed” e notificar (log ou fila de dead-letter).
- Não remover da Outbox até confirmação de sucesso na nuvem (ex.: 200 da Cloud API ou sucesso do BatchWrite no DynamoDB).

---

## 5. Preparação para escalabilidade futura

### 5.1 Evolução para SaaS completo

- **Fase atual:** Desktop com API local + sync pontual para DynamoDB.
- **Próximo passo:** Cloud API como ponto central de sync e configuração; todos os tenants sincronizam com a mesma Cloud API; TenantId em todo o fluxo.
- **Depois:** Opcionalmente, cliente web ou outro cliente; mesmo Cloud API; desktop continua com API local e sync para a mesma Cloud API.

### 5.2 API Gateway (futuro)

- Colocar um API Gateway (AWS API Gateway, Azure API Management ou similar) na frente da Cloud API: autenticação JWT, rate limit por tenant, roteamento, logging.
- Útil quando houver múltiplos consumidores (desktop, web, mobile) e necessidade de políticas centralizadas.

### 5.3 Microserviços (quando necessário)

- Enquanto o domínio for coeso (estoque, medicamentos, produtos, insumos), manter monolito modular (Application + Infrastructure) é mais simples.
- Se surgirem bounded contexts claros (ex.: faturamento, integrações externas, relatórios pesados), extrair para serviços separados que consomem eventos ou APIs; sync pode virar um serviço dedicado.

### 5.4 Configuração multi-cliente (feature flags e parâmetros por tenant)

- Armazenar por tenant: features habilitadas (ex.: “modulo_relatorios”), limites (ex.: max itens), branding (nome, logo).
- Serviço `ITenantSettingsService` (local lê do cache ou da Cloud API); Cloud API persiste em DynamoDB ou tabela de configuração; Client e LocalApi consultam para adaptar UI e regras.

---

## 6. Roadmap técnico evolutivo (sem reescrita)

### Fase 1 — Organização interna das camadas (curto prazo)

- **Objetivo:** Separar Application e Data dentro da solution sem mudar comportamento.
- **Ações:**
  - Criar projetos **Domain**, **Application**, **Infrastructure**.
  - Mover entidades “puras” para Domain (sem anotações EF/DynamoDB); manter modelos de persistência em Infrastructure (ou em Backend até a migração).
  - Mover Services de negócio para Application; interfaces de repositório em Application; implementações em Infrastructure.
  - Backend (renomear para Backend.LocalApi) referencia Application e Infrastructure; registra repositórios e serviços no DI.
- **Resultado:** Backend.LocalApi continua sendo o único host; Frontend inalterado; comportamento idêntico.

### Fase 2 — Padronização da API local (curto prazo)

- **Objetivo:** Controllers apenas com DTOs; tenant pronto para ser injetado.
- **Ações:**
  - Controllers só recebem/retornam DTOs; conversão DTO ↔ Model no Application (services).
  - Remover código morto (ex.: scope de IMedicamentosService nos controllers).
  - Introduzir **ITenantContext** (scoped) e middleware que preenche o tenant a partir do JWT (claim ou usuário); para já usar um tenant fixo “default” se não houver claim.
- **Resultado:** API local padronizada e pronta para receber TenantId real quando o Cognito tiver o claim.

### Fase 3 — Implementação do sync cloud (médio prazo)

- **Objetivo:** Sync tenant-aware e, se possível, incremental.
- **Ações:**
  - Adicionar **TenantId** em todas as entidades (local e DynamoDB); migrations e ajustes no DynamoDB (partition key).
  - Refatorar SyncService para filtrar por TenantId (local e DynamoDB Query por tenant); remover Scan global.
  - Opcional: implementar **SyncOutbox** e sync baseado em mudanças; SyncWorker (in-process) processa a Outbox e envia para DynamoDB (ou para a Cloud API quando existir).
  - Retry e tratamento de erro no sync; não travar a UI.
- **Resultado:** Sync seguro por tenant e mais escalável.

### Fase 4 — Habilitação multi-tenant (médio prazo)

- **Objetivo:** Suportar mais de um tenant (mesmo que inicialmente uma instalação use um só).
- **Ações:**
  - Cognito: atributo `custom:tenantId` (ou mapeamento usuário → tenant na base).
  - Resolução de tenant no login e em toda requisição; testes de isolamento (um tenant não vê dados de outro).
  - TenantSettings (arquivo ou tabela) com feature flags e parâmetros por tenant; serviço de configuração na Application.
- **Resultado:** Sistema multi-tenant funcional; novos clientes recebem TenantId no onboarding.

### Fase 5 — Preparação SaaS (longo prazo)

- **Objetivo:** Cloud API como ponto central; opção de cliente web; operação híbrida (desktop offline + cloud).
- **Ações:**
  - Criar projeto **Backend.CloudApi**; expor endpoints de sync (push/pull por tenant), configuração e saúde.
  - Migrar persistência cloud para ser acessada apenas pela Cloud API (DynamoDB ou RDBMS); SyncWorker ou LocalApi envia mudanças para a Cloud API em vez de escrever direto no DynamoDB.
  - Opcional: API Gateway na frente da Cloud API; cliente web consumindo a mesma Cloud API.
  - Documentar arquitetura e runbooks de deploy.
- **Resultado:** Arquitetura híbrida local + cloud pronta para crescimento SaaS e múltiplos clientes.

---

## Resumo do Blueprint

- **Arquitetura-alvo:** 3-Tier híbrido com Client.Desktop, Backend.LocalApi, Backend.CloudApi (futuro), SyncWorker (opcional), e camadas Domain, Application, Infrastructure, Shared.
- **Multi-tenant:** TenantId em todas as entidades; resolução no login/JWT; isolamento em todas as queries e no DynamoDB (partition key).
- **Sync:** Change tracking (Outbox) + sync por tenant; retry e política de conflitos (LWW ou versão); evolução para Cloud API como destino do sync.
- **Roadmap:** 5 fases incrementais (organização de camadas → padronização API → sync cloud → multi-tenant → SaaS), sem reescrita total, mantendo compatibilidade com a operação atual offline-first.

Este blueprint deve ser lido em conjunto com o **Relatório Técnico de Arquitetura** (`RELATORIO-TECNICO-ARQUITETURA.md`) para o contexto completo da análise e das recomendações.
