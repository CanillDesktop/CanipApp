# CanilApp

Sistema para controle de **medicamentos, insumos e operações**, projetado com arquitetura moderna para evolução futura como **SaaS multi-tenant**, mantendo funcionamento **offline-first** com sincronização em nuvem.

---

## Visão Geral

O **CanilApp** é uma aplicação desktop construída com:

* **Frontend:** .NET MAUI + Blazor WebView
* **Backend:** ASP.NET Core 8 (API local)
* **Banco local:** SQLite (EF Core)
* **Cloud (futuro/atual parcial):** AWS (Cognito + DynamoDB)

A aplicação foi desenhada para:

* Funcionar localmente (offline-first)
* Sincronizar dados com a nuvem
* Suportar múltiplos tenants (multi-tenant SaaS)

---

## 🧱 Arquitetura

Arquitetura em camadas com separação clara de responsabilidades:

```
Client (MAUI + Blazor)
   ↓
Local API (ASP.NET Core)
   ↓
Application / Domain / Infrastructure
   ↓
SQLite (local) + DynamoDB (cloud)
```

### Principais conceitos:

* **3-Tier híbrido (local + cloud)**
* **Multi-tenant via TenantId**
* **JWT (AWS Cognito) para autenticação**
* **Sync por change tracking (Outbox pattern)**

---

## 📁 Estrutura do Projeto

```
Client.Desktop       → Frontend (MAUI + Blazor)
Backend.LocalApi     → API local (core do sistema)
Backend.CloudApi     → API cloud (futuro / sync)
Backend.SyncWorker   → Sincronização (opcional)
Application          → Casos de uso
Domain               → Entidades e regras de negócio
Infrastructure       → Persistência (EF Core / DynamoDB)
Shared               → DTOs e contratos
```

---

## 🔐 Autenticação

O sistema utiliza **AWS Cognito**:

* Login via `USER_PASSWORD_AUTH`
* Backend valida JWT
* Extração de `TenantId` via claims
* Controle multi-tenant baseado no contexto

---

## 💾 Banco de Dados

### Local

* SQLite (por instalação)
* Todas as tabelas possuem `TenantId`
* Gerenciado via **Entity Framework Core**

### Cloud (parcial/futuro)

* DynamoDB
* Partition key: `TenantId`
* Fonte da verdade para sincronização

---

## 🔄 Sincronização

Estratégia baseada em:

* **Change tracking (Outbox)**
* Sync por tenant
* Push/Pull incremental
* Resolução de conflitos: *last-write-wins* (inicial)

---

## ⚙️ Pré-requisitos

* .NET 8 SDK
* Visual Studio 2022 (com MAUI)
* Git
* (Opcional) AWS CLI

---

## ▶️ Como rodar o projeto

### 1. Clonar repositório

```bash
git clone <URL_DO_REPOSITORIO>
cd CanilApp
```

### 2. Restaurar dependências

```bash
dotnet restore CanilApp.sln
```

### 3. Configurar backend

Criar arquivo:

```
Backend/appsettings.json
```

Exemplo mínimo:

```json
{
  "AWS": {
    "Region": "us-east-1",
    "UserPoolId": "SEU_USER_POOL",
    "ClientId": "SEU_CLIENT_ID",
    "IdentityPoolId": "SEU_IDENTITY_POOL"
  }
}
```

Ou usar:

```bash
dotnet user-secrets set "AWS:Region" "us-east-1"
```

---

### 4. Rodar migrations

```bash
cd Backend
dotnet ef database update
```

---

### 5. Rodar backend

```bash
dotnet run
```

Swagger disponível em:

```
http://localhost:<porta>/swagger
```

---

### 6. Rodar frontend

* Defina o projeto `Client.Desktop` como inicial
* Execute via Visual Studio

---

## 🤝 Contribuição

1. Crie uma branch:

```bash
git checkout -b feature/minha-feature
```

2. Commit:

```bash
git commit -m "feat: descrição"
```

3. Push:

```bash
git push origin feature/minha-feature
```

---

## 📄 Licença

Definir conforme necessidade do projeto.

---
