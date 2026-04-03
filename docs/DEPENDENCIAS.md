# Dependências e versões — CanilApp

Resumo das frameworks e pacotes NuGet da solução `CanilApp.sln`, incluindo a stack **Blazor WebView** do Frontend.

**Última conferência com os `.csproj` e `dotnet list package --include-transitive` (projeto Frontend).**

---

## Frameworks por projeto

| Projeto | Target framework | Observações |
|---------|------------------|-------------|
| **Shared** | `net8.0` | Biblioteca; sem pacotes NuGet. |
| **Backend** | `net8.0` | `RuntimeIdentifier`: **win-x64**; `OutputType`: Exe; `AssemblyName`: Backend. |
| **Frontend** | `net8.0-windows10.0.19041.0` | MAUI; Windows mín. **10.0.17763** (`SupportedOSPlatformVersion` / `TargetPlatformMinVersion`). |
| **Frontend (Razor)** | `RazorLangVersion` **8.0** | Definido em `Frontend.csproj`. |

---

## WebView (Blazor no MAUI)

### Pacotes NuGet (Blazor WebView)

| Pacote | Versão | Origem |
|--------|--------|--------|
| **Microsoft.AspNetCore.Components.WebView.Maui** | **8.0.90** | Referência direta no `Frontend.csproj` (alinhado a `Microsoft.Maui.Controls` 8.0.90). |
| **Microsoft.AspNetCore.Components.WebView** | **8.0.0** | Transitiva (puxada por `WebView.Maui`). |

### Stack Windows relacionada (transitiva)

O alvo Windows do MAUI puxa, entre outros:

| Pacote | Versão |
|--------|--------|
| **Microsoft.WindowsAppSDK** | **1.5.240802000** |
| **Microsoft.Windows.SDK.BuildTools** | **10.0.22621.756** |

O Windows App SDK inclui integração com **WebView2** no ecossistema WinUI (headers/API nativa no pacote). Não há referência direta separada a `Microsoft.Web.WebView2` no grafo NuGet listado pelo `dotnet list` para este projeto.

### Runtime no Windows

Para o Blazor WebView no Windows, costuma ser necessário o **Microsoft Edge WebView2 Runtime** (distribuição *Evergreen*) instalado na máquina. A versão exata do runtime **não** está fixada no repositório; depende do que o utilizador tem instalado ou do instalador WebView2.

---

## Backend — pacotes NuGet (`Backend.csproj`)

| Pacote | Versão |
|--------|--------|
| AWSSDK.CognitoIdentity | 4.0.2.7 |
| AWSSDK.CognitoIdentityProvider | 4.0.4.5 |
| AWSSDK.DynamoDBv2 | 4.0.7.1 |
| AWSSDK.Extensions.NETCore.Setup | 4.0.3.5 |
| BCrypt.Net-Next | 4.0.3 |
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.20 |
| Microsoft.AspNetCore.OpenApi | 8.0.1 |
| Microsoft.EntityFrameworkCore.Design | 8.0.1 |
| Microsoft.EntityFrameworkCore.Sqlite | 8.0.1 |
| Microsoft.EntityFrameworkCore.Tools | 8.0.1 |
| Microsoft.VisualStudio.Web.CodeGeneration.Design | 8.0.7 |
| Serilog.AspNetCore | 9.0.0 |
| Serilog.Sinks.Console | 6.1.1 |
| Serilog.Sinks.File | 7.0.0 |
| Swashbuckle.AspNetCore | 6.6.2 |
| System.Text.Json | 9.0.9 |

**Referência de projeto:** `Shared`.

---

## Frontend — pacotes NuGet (`Frontend.csproj`)

| Pacote | Versão |
|--------|--------|
| AWSSDK.CognitoIdentityProvider | 4.0.4.4 |
| Microsoft.AspNetCore.WebUtilities | 8.0.0 |
| Microsoft.Maui.Controls | 8.0.90 |
| Microsoft.Maui.Controls.Compatibility | 8.0.90 |
| Microsoft.AspNetCore.Components.WebView.Maui | 8.0.90 |
| Microsoft.Extensions.Http | 8.0.1 |
| CommunityToolkit.Mvvm | 8.4.0 |
| sqlite-net-pcl | 1.9.172 |
| Microsoft.AspNetCore.Components.Authorization | 8.0.11 |

**Referência de projeto:** `Shared`.

---

## Outras notas

- **Node.js / npm:** não há dependências de build no repositório para o app principal (apenas NuGet).
- **LibMan:** `Backend/libman.json` existe com lista de bibliotecas vazia.
- **Diferença de patch AWS:** Backend usa `AWSSDK.CognitoIdentityProvider` **4.0.4.5**; Frontend **4.0.4.4**.

### Atualizar a lista transitiva localmente

```powershell
dotnet list Frontend/Frontend.csproj package --include-transitive
dotnet list Backend/Backend.csproj package --include-transitive
```

Use isto após upgrades de pacotes para rever versões efetivas (incluindo WebView / Windows SDK).
