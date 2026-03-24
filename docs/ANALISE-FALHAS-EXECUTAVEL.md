# Análise de falhas na execução do executável (CanilApp)

**Objetivo:** Identificar causas de falha após gerar o executável, especialmente “backend inicia mas frontend não abre” e “não funciona em outras máquinas”.

---

## 1. Verificação se o problema ainda persiste

### 1.1 Problema crítico corrigido (caminho absoluto)

**Estado anterior:** Em `Frontend\Services\BackendStarter.cs` (linha 98) havia um **caminho absoluto fixo**:

```csharp
var baseDir = "C:\\Users\\Arthu\\source\\repos\\CanillDesktop\\CanipApp\\Frontend\\bin\\Debug\\net8.0-windows10.0.19041.0\\win10-x64\\Backend\\win-x64";
```

**Efeito:** Em qualquer máquina ou pasta diferente (incluindo publicação), o Backend não era encontrado → `FileNotFoundException` em `StartBackendAndGetUrl()` → exceção em `MauiProgram.CreateMauiApp()` **antes** da janela do app ser criada. Isso explica o cenário “backend iniciava corretamente, mas o frontend não abria”: na verdade o frontend **falhava na inicialização** ao tentar localizar o Backend, e a exceção era lançada antes da UI aparecer.

**Correção aplicada:** O `BackendStarter` foi alterado para **não usar mais caminho fixo**. Agora ele:

- Usa `AppContext.BaseDirectory` como diretório da aplicação.
- Monta uma lista de **diretórios candidatos** (pasta do exe, pasta pai, `Backend`, `Backend\win-x64`, etc.).
- Para cada candidato, procura `Backend.exe` ou `Backend.dll` (e combinações com subpastas).
- Funciona tanto no layout de **Debug** (ex.: `win10-x64\` com `..\Backend\win-x64\`) quanto no layout de **Publish** (ex.: pasta única com `Backend\Backend.exe`).

Com essa alteração, o problema de “frontend não abre” por causa de caminho do Backend **deve deixar de ocorrer** em outras pastas e máquinas, desde que a pasta **Backend** esteja junto ao executável (como já fazem os targets de Build/Publish do Frontend).

### 1.2 Outros pontos que podem ainda causar falhas

- **Credenciais/configuração AWS** em outras máquinas (veja seção 2).
- **Estrutura de publicação** (Backend não copiado ou em subpasta inesperada) — validar com a seção 4.
- **Runtime .NET** não instalado na máquina (Backend está `SelfContained=false`).

---

## 2. Possíveis causas técnicas priorizadas por probabilidade

### Alta probabilidade (já tratada ou a verificar primeiro)

| # | Causa | Onde está no código/config | Status |
|---|--------|----------------------------|--------|
| 1 | **Caminho absoluto para o Backend** | `Frontend\Services\BackendStarter.cs` — variável `baseDir` (antes linha 98) | **Corrigido:** passou a usar `AppContext.BaseDirectory` e candidatos relativos. |
| 2 | **Backend não encontrado no layout de publicação** | Targets `CopyBackendAfterBuild` e `CopyBackendAfterPublish` no `Frontend\Frontend.csproj`; estrutura de pastas após publish | Ver seção 4. Garantir que o publish do Frontend rode e que a pasta `Backend` exista ao lado do exe. |
| 3 | **Configuração AWS ausente em outras máquinas** | `Backend\appsettings.json` (AWS:Region, UserPoolId, ClientId, IdentityPoolId); User Secrets só na máquina de dev | Se o appsettings não for copiado ou for sobrescrito, o Backend lança na inicialização (`Program.cs` linhas 126–129). Em outras máquinas, garantir que `appsettings.json` esteja na pasta publicada do Backend. |

### Média probabilidade

| # | Causa | Onde está no código/config | Sugestão |
|---|--------|----------------------------|----------|
| 4 | **.NET Runtime não instalado** | `Backend\Backend.csproj`: `<SelfContained>false</SelfContained>` | Na máquina destino é necessário .NET 8 Runtime (Windows). Ou publicar Backend como self-contained. |
| 5 | **SecureStorage / comportamento em publicação** | Login e `AuthDelegatingHandler` usam `SecureStorage` para tokens | Em Windows publicado, SecureStorage costuma usar DPAPI; pode variar por usuário/máquina. Se houver falha de login só em publicado, investigar permissões e armazenamento. |
| 6 | **Exceção não tratada na inicialização** | `MauiProgram.CreateMauiApp()` chama `BackendStarter.StartBackendAndGetUrl()` e faz `throw` em caso de falha | Qualquer exceção em `StartBackendAndGetUrl()` (ex.: Backend não encontrado, timeout) impede a abertura da janela. A mensagem pode aparecer só no console ou em log. Com a correção do caminho, a causa mais provável dessa exceção foi removida. |
| 7 | **Kill do Backend antes do build** | `Frontend\Frontend.csproj`: target `KillBackendBeforeBuild` executa `taskkill /IM Backend.exe /F` | Pode encerrar qualquer processo chamado “Backend.exe” no sistema. Em máquinas com outro “Backend.exe”, pode causar efeitos colaterais. Considerar remover ou restringir (ex.: só em dev). |

### Menor probabilidade (mas relevantes)

| # | Causa | Onde está no código/config | Sugestão |
|---|--------|----------------------------|----------|
| 8 | **CORS** | Backend escuta em `127.0.0.1`; CORS em `Program.cs` permite localhost/127.0.0.1/LAN | Não costuma ser problema para app desktop que fala só com o Backend local. |
| 9 | **URL do frontend / BaseAddress** | `MauiProgram.cs`: `BackendConfig.Url` vem de `StartBackendAndGetUrl()` | Se o Backend subir em outra porta ou não subir, o cliente já falha ao iniciar (timeout ou exceção). Com o caminho corrigido, o Backend tende a ser encontrado e iniciado. |
| 10 | **Variáveis de ambiente** | Backend usa `builder.Configuration` (appsettings + ambiente) | AWS pode ser configurada por variáveis de ambiente; se na máquina de destino não houver appsettings nem env, o Backend falha na startup. |
| 11 | **Recursos estáticos (fontes, imagens)** | MAUI: `MauiImage`, `MauiFont`, `MauiAsset` no `Frontend.csproj` | Em publish, normalmente incluídos. Se a tela não carregar, verificar se há erros de recurso não encontrado. |

---

## 3. Onde exatamente está cada problema (referências de código)

### 3.1 BackendStarter — localização do Backend (corrigido)

- **Arquivo:** `Frontend\Services\BackendStarter.cs`
- **Antes:** Uma única variável `baseDir` com caminho absoluto da máquina do desenvolvedor; todos os `possiblePaths` derivavam desse `baseDir`.
- **Agora:** `appDir = AppContext.BaseDirectory`; vários `candidateBaseDirs` (appDir, parent, `Backend`, `Backend\win-x64`, etc.); para cada candidato são testados `Backend.exe` e `Backend.dll` em várias combinações. Assim funciona em dev (ex.: `win10-x64\` com `..\Backend\win-x64`) e em publish (ex.: pasta única com `Backend\`).

### 3.2 Inicialização do app e “frontend não abre”

- **Arquivo:** `Frontend\MauiProgram.cs`
- **Trecho:** Entre as linhas 17–27, `backendUrl = BackendStarter.StartBackendAndGetUrl()` é chamado dentro de `try`; em caso de exceção, é feito `throw new Exception(...)`. Isso ocorre **antes** de `MauiApp.CreateBuilder()` e de qualquer janela. Por isso qualquer falha em `StartBackendAndGetUrl()` (Backend não encontrado ou timeout) faz o app terminar sem abrir a janela.

### 3.3 Configuração AWS obrigatória no Backend

- **Arquivo:** `Backend\Program.cs`
- **Linhas 126–129:**  
  `builder.Configuration["AWS:Region"]`, `UserPoolId`, `ClientId` são lidos e, se ausentes, é lançada `InvalidOperationException`. A configuração vem de `appsettings.json` (e sobrescritas por ambiente). Em outra máquina, se a pasta publicada do Backend não tiver `appsettings.json` (ou a seção AWS), o Backend falha ao iniciar.

### 3.4 Banco e logs (já portáteis)

- **Arquivo:** `Backend\Program.cs`
- **Linhas 116–121, 335–338:** Uso de `Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)` para `CanilApp` (DB e logs). Isso é portável entre máquinas e usuários.

### 3.5 Login e sessão

- **Arquivos:** `Frontend\ViewModels\LoginViewModel.cs`, `Frontend\Handlers\AuthDelegatingHandler.cs`
- **Fluxo:** Login chama `api/login` (Backend usa Cognito); tokens são salvos em `SecureStorage` e usados no `AuthDelegatingHandler`. Não há uso de caminhos absolutos nem de cookies no Backend para sessão; a “sessão” é o token no SecureStorage. Em outras máquinas, o ponto crítico é o Backend estar acessível e com AWS configurada; o SecureStorage no Windows costuma funcionar por usuário.

### 3.6 Cópia do Backend no Build/Publish

- **Arquivo:** `Frontend\Frontend.csproj`
- **CopyBackendAfterBuild (após Build):** Copia `Backend\bin\$(Configuration)\net8.0\**` para `$(OutputPath)Backend\`. No Windows MAUI, `OutputPath` pode ser `bin\Debug\net8.0-windows10.0.19041.0\`, e o exe pode rodar em `...\win10-x64\`; nesse caso o Backend fica em `...\Backend\` (com possível subpasta `win-x64` conforme o Backend.csproj).
- **CopyBackendAfterPublish (após Publish):** Publica o Backend em `Backend\bin\$(Configuration)\net8.0\publish\` e copia o conteúdo para `$(PublishDir)Backend\`. O executável do Frontend costuma estar em `PublishDir`; o `BackendStarter` agora procura também em `AppContext.BaseDirectory` e `Backend` dentro dela, cobrindo esse layout.

---

## 4. Sugestões de correção objetivas

### 4.1 Já aplicada

- **BackendStarter:** Uso de `AppContext.BaseDirectory` e lista de diretórios candidatos para localizar `Backend.exe`/`Backend.dll`, sem caminho absoluto. Assim o executável funciona em qualquer pasta e em outras máquinas, desde que a pasta Backend esteja junto ao exe.

### 4.2 Recomendações adicionais

1. **Garantir appsettings.json do Backend na publicação**
   - O SDK Web já costuma incluir `appsettings.json` no publish. Confirmar que, na pasta publicada do Backend (copiada para dentro do Frontend), existem `appsettings.json` e, se necessário, `appsettings.Production.json` com a seção `AWS` (Region, UserPoolId, ClientId, IdentityPoolId). Em máquinas sem User Secrets, essa é a única fonte de configuração.

2. **Documentar requisitos para outras máquinas**
   - .NET 8 Runtime (Windows) instalado, ou publicar Backend como self-contained.
   - Instrução de que a pasta `Backend` deve estar no mesmo diretório (ou na estrutura esperada) em relação ao executável do Frontend; não mover só o .exe do Frontend.

3. **Opcional: Backend self-contained**
   - No `Backend\Backend.csproj`, definir `<SelfContained>true</SelfContained>` no publish (ou via linha de comando) para não depender do runtime instalado na máquina. Aumenta o tamanho do pacote.

4. **Opcional: KillBackendBeforeBuild**
   - O target `KillBackendBeforeBuild` com `taskkill /IM Backend.exe /F` é agressivo. Considerar remover ou condicionar a `$(Configuration)` == Debug para evitar encerrar outros processos em build de Release ou em CI.

5. **Logging em caso de falha na inicialização**
   - Se quiser mais visibilidade em máquinas onde “nada abre”, pode-se logar a exceção de `StartBackendAndGetUrl()` em arquivo (ex.: em `%LocalApplicationData%\CanilApp\`) antes de relançar, ou exibir uma mensagem em uma janela mínima antes de encerrar.

---

## 5. Validação do processo de build/publicação para outras máquinas

### 5.1 Build (desenvolvimento)

- Rodar Build do Frontend (ex.: `dotnet build Frontend\Frontend.csproj -f net8.0-windows10.0.19041.0`).
- Verificar se existe a pasta **Backend** em:
  - `Frontend\bin\Debug\net8.0-windows10.0.19041.0\Backend\`
  - e, se o Backend for compilado com RID, algo como `Backend\win-x64\Backend.exe`.
- O `BackendStarter` agora procura a partir de `AppContext.BaseDirectory` (que em execução pode ser `...\win10-x64\`), então `..\Backend\win-x64\Backend.exe` será encontrado se a cópia estiver nesse layout.

### 5.2 Publish (executável para distribuição)

- Publicar o Frontend para Windows, por exemplo:
  - `dotnet publish Frontend\Frontend.csproj -c Release -f net8.0-windows10.0.19041.0`
- O target **CopyBackendAfterPublish** deve:
  1. Publicar o Backend em `Backend\bin\Release\net8.0\publish\`.
  2. Copiar todo o conteúdo dessa pasta para `Frontend\bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Backend\` (ou equivalente).
- **Checklist:**
  - Na pasta de publish do Frontend existe **Backend\**.
  - Dentro de **Backend\** existem **Backend.exe** (ou Backend.dll), **appsettings.json**, **Backend.dll** e dependências.
  - O executável do Frontend (ex.: **Frontend.exe**) está na raiz da pasta de publish (ou no mesmo nível que a pasta **Backend**).
- Copiar a **pasta inteira** de publish para outra máquina (não só o .exe do Frontend). Executar o .exe do Frontend a partir dessa pasta; o `AppContext.BaseDirectory` será o diretório onde está o exe, e o `BackendStarter` procurará `Backend\Backend.exe` (ou `Backend\win-x64\Backend.exe`) em relação a ele.

### 5.3 Teste em máquina limpa

- Em outra máquina (ou VM) **sem** o repositório e **sem** Visual Studio:
  - Instalar **.NET 8 Runtime (Windows)** se o Backend for framework-dependent.
  - Copiar a pasta de publish completa.
  - Executar o executável do Frontend.
  - Se o Backend não iniciar, verificar se na pasta há `Backend\` e se nela está `appsettings.json` com a seção AWS. Se faltar runtime, instalar ou publicar Backend como self-contained.

---

## 6. Resumo

- **Causa principal do “frontend não abria”** era o **caminho absoluto** em `BackendStarter.cs` para localizar o Backend, o que quebrava em qualquer máquina ou pasta diferente. Isso foi **corrigido** com base em `AppContext.BaseDirectory` e diretórios candidatos.
- **Para funcionar em outras máquinas:** (1) Distribuir a pasta de publish inteira; (2) garantir que `appsettings.json` do Backend esteja na pasta publicada com a configuração AWS; (3) ter .NET 8 Runtime na máquina ou publicar Backend como self-contained.
- **Login/sessão:** Dependem do Backend estar acessível e da configuração AWS; tokens em SecureStorage são portáteis por usuário/máquina. Não foram encontrados caminhos absolutos nem dependências de cookies no fluxo de login que impeçam execução em outras máquinas.

Com a correção aplicada no `BackendStarter` e seguindo o processo de build/publish e a checklist acima, o executável tende a funcionar tanto na máquina de desenvolvimento quanto em outras máquinas.
