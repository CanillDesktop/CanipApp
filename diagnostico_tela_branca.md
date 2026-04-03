# Diagnóstico — Tela branca em Blazor WebView

## Resumo

Neste repositório o cliente é **.NET MAUI 8** com **Blazor** embutido via `BlazorWebView`, no Windows usando o motor **WebView2** (componente de tempo de execução embutido do Microsoft Edge). A área da interface está dentro de uma `ContentPage` com **fundo branco fixo**. Se o controle híbrido não pintar o documento ou o tempo de execução do WebView2 falhar antes do Blazor substituir o `#app`, o utilizador vê **apenas branco** — sensação de “WebView2 não iniciou”.

As causas **mais alinhadas ao código** são: (1) **tempo de execução ou ambiente do WebView2** fora do repositório; (2) **falha ao carregar** `wwwroot/index.html` ou o ficheiro `_framework/blazor.webview.js` gerado na **compilação**; (3) **navegação forçada** com recarregamento completo da página em rotas iniciais; (4) **cor de fundo branca** que mascara qualquer atraso ou falha parcial. Falhas de **API local** ou **Cognito** não explicam tela branca desde o primeiro instante: nesse caso o fluxo normal mostra `StartupFailurePage` (sem Blazor) ou a página de **início de sessão** dentro do WebView.

---

## Pipeline de renderização (obrigatório)

Ordem lógica no **MAUI Blazor WebView** (Windows): o anfitrião nativo cria o controlo → o motor **WebView2** arranca → carrega o **documento anfitrião** (`HostPage`) → o HTML pede recursos (CSS, `theme.js`, depois `blazor.webview.js`) → o anfitrião **.NET** invoca o arranque do Blazor (porque `autostart="false"`) → o componente raiz (`Routes`) monta no seletor `#app` → o **roteador** resolve a rota e pinta a página.

| Etapa | Pergunta | Estado de validação neste repositório |
|-------|----------|--------------------------------------|
| A | O WebView2 **inicializou** (núcleo pronto, sem falha nativa)? | **Ponto de falha crítico não validado.** O projeto **não** regista eventos nem verificações explícitas em `MainPage.xaml.cs`; só o tempo de execução e o MAUI sabem. |
| B | O `index.html` **foi carregado** no controlo? | **Ponto de falha crítico não validado.** Esperado quando `HostPage="wwwroot/index.html"` resolve corretamente; não há código que confirme `NavigationCompleted` ou equivalente. |
| C | O `blazor.webview.js` **foi obtido e executado**? | **Ponto de falha crítico não validado.** Sem rede de depuração ou consola do motor, não se prova. Se o ficheiro ou a pasta `_framework` faltar na saída, esta etapa **falha** e o Blazor **nunca** liga. |
| D | O `#app` foi **substituído** pelo conteúdo Blazor? | **Ponto de falha crítico não validado.** Só ocorre após C com sucesso. Até lá, o DOM pode ficar só com `<div id="app"><div class="spinner"></div></div>` — sem estilo visível forte, parece **vazio**; por baixo, a `ContentPage` é **branca**, logo **tela branca percebida**. |

**Conclusão do pipeline:** o repositório **não permite afirmar** em qual de A–D falhou **sem instrumentação** (ferramentas de programador do WebView, registo de navegação, ou inspeção da pasta de saída). A análise estática aponta **C e A** como as etapas mais frágeis em ambientes reais (ficheiros em falta / tempo de execução).

---

## Falhas de arranque do Blazor (bootstrap)

- **Antes do primeiro render:** o grafo começa em `Routes.razor` com `CascadingAuthenticationState` e `Router`. O `AuthorizeRouteView` chama `GetAuthenticationStateAsync` → `AuthenticationStateService.GetUserClaimsAsync()` (leitura do armazenamento seguro, sem rede). **Não há** no código analisado um `throw` óbvio nesse caminho; falha aqui seria **incerta** e tendencialmente mostraria interface de erro do Blazor, não silêncio total.
- **`OnInitialized` na rota `/`:** `Index.razor` faz `Navigation.NavigateTo(..., forceLoad: true)` imediatamente. Isso ocorre **depois** do primeiro arranque do circuito, mas força **recarga completa do documento**. O ponto de quebra possível é **reentrada no pipeline A–D** com janela em que o utilizador vê só fundo — **incerto, mas possível** conforme versão do MAUI/WebView2.
- **`OnAfterRender`:** nas páginas de entrada (`LoginPage`, `LoginLayout`) não há lógica pesada obrigatória no primeiro ciclo que impeça pintura.
- **Exceções silenciosas:** `CustomAuthenticationStateProvider.HandleAuthenticationStateChanged` usa `async void`; erros assíncronos podem **não** chegar à interface de forma clara, mas isso costuma atuar **após** eventos de sessão, não no primeiro arranque a frio.

**Síntese:** a causa de tela **totalmente** branca desde o início é **pouco compatível** com exceção só no bootstrap dos componentes **.NET** deste projeto; é **muito mais compatível** com falha **antes** do Blazor ligar (motor WebView2 ou ficheiro `blazor.webview.js`).

---

## JavaScript (prioridade alta)

| Ficheiro / origem | Papel no primeiro paint | Risco para tela branca |
|-------------------|-------------------------|-------------------------|
| `_framework/blazor.webview.js` | Obrigatório para o anfitrião ligar o Blazor ao DOM. | **Crítico.** Falha = `#app` nunca atualizado de forma significativa pelo framework. |
| `wwwroot/js/theme.js` | Executa no `<head>`, IIFE: `localStorage`, `matchMedia`, classes no `documentElement`. | **Baixo** em ambientes normais. **Incerto** só com políticas extremas que impeçam armazenamento local ou que lancem no motor. |
| Folha externa (ícones Bootstrap via rede) | Carregamento paralelo; não bloqueia a análise do `<script>` no final do `<body>` de forma típica. | **Baixo** para branco total; afeta ícones. |
| **JSInterop** na primeira rota visível (`/login`) | `LoginPage` só chama o motor JavaScript **após** navegação pós-login (evento), não no `OnInitialized`. | **Não** é candidato a bloquear o **primeiro** render da tela de início de sessão. |
| `ThemeToggle` em `NavMenu` | Usa `theme.get` / `theme.toggle`; o menu lateral **não** compõe o `LoginLayout`. | **Não** entra no primeiro ecrã de `/login`. |

---

## WebView2 (aprofundamento)

- **Colisão ou encerramento do processo do motor:** pode deixar o retângulo do `BlazorWebView` vazio; o projeto **não** trata nem regista — **ponto de falha crítico não validado** em tempo de execução.
- **Pasta de dados do utilizador (`UserDataFolder`):** o CanilApp **não** define pasta personalizada no código analisado; usa o comportamento padrão do MAUI/WebView2. Corrupção ou permissões na pasta padrão do perfil Windows podem impedir o núcleo — causa **externa ao código**, **plausível** em suporte.
- **Tempo de execução incompatível ou ausente:** sem o componente Evergreen correto, o controlo pode não renderizar conteúdo web — **causa número 1** em instalações limpas (ver secção seguinte).
- **Antivírus / política:** bloqueio do executável do motor (`msedgewebview2.exe` ou filhos) produz o mesmo sintoma visual — **externo ao código**.

---

## Causa mais provável

**A causa número 1, com maior peso técnico, é a falha na camada “motor WebView2 + ficheiros estáticos do Blazor” (etapas A e C do pipeline), não a lógica de negócio nem o Cognito.**

**Justificativa:** (1) O código **não** mostra outro ponto que impeça **todo** o desenho antes do primeiro componente Razor — o fluxo de autenticação e a API local atuam **depois**. (2) Tela **inteiramente** branca combina com **retângulo web vazio** + `BackgroundColor="White"` na `ContentPage`. (3) Sem `blazor.webview.js` ou com núcleo WebView2 inoperante, o marcador `#app` permanece estático e o utilizador **não** vê formulário de início de sessão. (4) O projeto **não valida** A–D; na prática, a primeira verificação física em máquinas com problema costuma ser **tempo de execução WebView2** ou **conteúdo `_framework` em falta** na pasta publicada.

A **segunda** hipótese **específica deste repositório** é o par **`NavigateTo(..., forceLoad: true)`** em `Index.razor` / `RedirectToLogin.razor`, que pode **reiniciar** o documento e criar intervalo ou estado estranho — útil a testar **depois** de confirmar WebView2 e ficheiros.

---

## Fluxo de execução esperado

1. Arranque do processo WinUI/MAUI → `MauiProgram.CreateMauiApp()` (inclui `BackendStarter`; não bloqueia o WebView se não lançar exceção não tratada antes de `MainPage`).
2. `App` define `MainPage` = `MainPage` (se não houver falha gravada do backend).
3. `MainPage` carrega o XAML: `BlazorWebView` com `HostPage="wwwroot/index.html"` e raiz `Routes` em `#app`.
4. O MAUI pede ao WebView2 que carregue o HTML anfitrião a partir dos recursos embutidos/publicados.
5. O navegador embutido aplica `<head>`, executa `theme.js`, constrói o `<body>` com `#app` e o marcador inicial.
6. O anfitrião .NET carrega e executa o arranque via `blazor.webview.js` (`autostart="false"` é normal).
7. O Blazor monta `Routes` → `Router` resolve `/` → `Index.razor` redireciona para `/login` ou `/home`.
8. Utilizador vê `LoginPage` (layout de início de sessão) ou `Home` com `MainLayout`, conforme estado.

---

## Onde quebra

| Sintoma relatado | Etapa do fluxo esperado onde o pipeline **para** |
|------------------|---------------------------------------------------|
| Janela abre, área central **uniformemente branca**, nunca aparece título “CanilApp - Login” | **Entre 3 e 6** (predominantemente **4–6**): motor WebView2 inativo, `index.html`/`blazor.webview.js` inacessível, ou arranque do Blazor não concluído. O utilizador vê sobretudo a cor da `ContentPage`. |
| Aparece “Verificando autenticação...” e **trava** | **7**, sub-etapa `AuthorizeRouteView` / fornecedor de estado (menos comum como “branco total”). |
| Conteúdo aparece e **some** após um instante | **7**, suspeita de `forceLoad: true` a recarregar o documento (**incerto** até reproduzir). |

**Ponto exato de quebra sem depuração:** **indeterminado no código-fonte** — é necessariamente **entre a criação do `BlazorWebView` e a conclusão da primeira montagem bem-sucedida de `Routes` no DOM**, com **maior densidade de probabilidade** na transição **carregamento do documento → execução de `blazor.webview.js` → primeiro render**.

---

## Problemas críticos

| # | Problema | Evidência no código |
|---|-----------|---------------------|
| 1 | **Nenhum tratamento explícito da inicialização do WebView2** | `Frontend/MainPage.xaml.cs` só chama `InitializeComponent()`; não há subscrição a eventos do controle nativo nem verificação pós-inicialização. Falhas do motor ficam **sem registo na aplicação**. |
| 2 | **Fundo branco na página que hospeda o WebView** | `Frontend/MainPage.xaml` define `BackgroundColor="White"`. Qualquer falha de renderização no retângulo do Blazor deixa a janela **visualmente toda branca**. |
| 3 | **Recarregamento completo da página na rota inicial** | `Frontend/Components/Pages/Index.razor` e `Frontend/Components/RedirectToLogin.razor` usam `Navigation.NavigateTo(..., forceLoad: true)`. Em cenários híbridos, recarregar a página inteira pode **reiniciar** o documento e o arranque do Blazor de forma agressiva; em combinações de versão ou ambiente, isso pode contribuir para **clarão branco prolongado** ou estado inconsistente (marcar como **incerto, mas possível** até reproduzir com a versão exata do conjunto de ferramentas de desenvolvimento). |
| 4 | **Recurso referenciado em `index.html` ausente no `wwwroot`** | `Frontend/wwwroot/index.html` referencia `favicon.png`, mas **não existe** ficheiro correspondente na pasta `wwwroot` listada no repositório. Normalmente é só falha de **recurso não encontrado** no ícone; **incerto, mas possível** impacto em ferramentas ou políticas que bloqueiam recursos com falha em cadeia (raro para ícone). |

---

## Problemas potenciais

- **`blazor.webview.js` com `autostart="false"`** em `wwwroot/index.html`: o arranque é controlado pelo hospedeiro MAUI (comportamento esperado). Se o pacote ou a **compilação** estiver corrompida ou incompleta, o ficheiro JavaScript de arranque não executa e o `#app` permanece só com o marcador estático — **incerto** sem inspecionar a saída da compilação.
- **Classe `.spinner` no HTML inicial**: `index.html` contém `<div class="spinner"></div>` dentro de `#app`, mas **não há regra óbvia** para `.spinner` em `wwwroot/css/app.css` (trecho analisado). Enquanto o Blazor não montar, o utilizador pode ver **área vazia** (ainda sobre fundo branco da `ContentPage`).
- **Folha de estilos em rede externa**: `index.html` carrega ícones do Bootstrap a partir de um **servidor de rede de entrega de conteúdos** (`cdn.jsdelivr.net`). Sem rede ou com bloqueio, falha o carregamento do CSS externo — costuma afetar **ícones**, não apagar toda a interface; **incerto** em ambientes com inspeção estrita de conteúdo misto.
- **`LoginPage.razor` e `CadastroPage.razor`**: navegação **após início de sessão** via `InvokeVoidAsync` com execução dinâmica de código no motor JavaScript para alterar o endereço da página (`window.location.href`). **Não** explica tela branca no arranque; pode gerar erros **após** autenticação se o motor JavaScript não estiver pronto.
- **`ThemeToggle.razor`**: invoca `theme.get` / `theme.toggle` definidos em `wwwroot/js/theme.js`. Se o ficheiro JavaScript falhar antes de expor `window.theme`, chamadas subsequentes quebram — **após** o utilizador interagir, não na primeira pintura.
- **Publicação**: o `.csproj` do Frontend **não** fixa `PublishSingleFile` para o executável principal; o backend copiado usa `PublishSingleFile=false`. Se alguém publicar o Frontend com **ficheiro único** ou opções que não extraem bem os estáticos, **incerto, mas possível** perda de `wwwroot` ou de `_framework` na pasta de saída.
- **Ambiente**: tempo de execução WebView2 em falta, bloqueio por antivírus, política empresarial ou perfil de dados do WebView2 corrompido — **fora do código**, mas é a causa **mais frequente** de tela totalmente branca com MAUI Blazor no Windows.

---

## Análise por ficheiro

### `Frontend/MainPage.xaml`

- **Problema:** `BackgroundColor="White"` no `ContentPage`; `BlazorWebView` sem atributos adicionais de diagnóstico.
- **Por que causa tela branca:** O utilizador associa a janela inteira ao “nada a aparecer”. O branco vem da página nativa, não necessariamente do HTML.
- **Como corrigir:** Para diagnóstico, usar cor de fundo distinta na `ContentPage` ou no `BlazorWebView` para separar “área nativa” de “área web”. Em produção, manter aparência desejada após confirmar renderização.

### `Frontend/MainPage.xaml.cs`

- **Problema:** Construtor vazio além de `InitializeComponent()`; **não** existe uso do padrão de espera explícita pelo núcleo do WebView2 nem manipuladores de eventos de inicialização ou falha expostos pela API do MAUI para cenários avançados.
- **Por que causa tela branca:** Não causa diretamente; **impede** detetar e registar falhas do motor ou do anfitrião.
- **Como corrigir:** Quando suportado pela versão do pacote, associar eventos do `BlazorWebView` (por exemplo criação do núcleo, falhas de navegação) e escrever em log ou mostrar uma página nativa de erro.

### `Frontend/App.xaml.cs`

- **Problema:** Escolhe `MainPage` ou `StartupFailurePage` conforme `StartupDiagnostics.BackendFailureUserMessage`.
- **Por que causa tela branca:** Se a mensagem de falha do backend **não** for preenchida (URL inválida `http://127.0.0.1:1` em `MauiProgram` sem exceção), abre-se `MainPage` com WebView mesmo com API inutilizável — a interface Blazor pode ainda aparecer; **não** é o cenário típico de branco total por backend.
- **Como corrigir:** Se quiser evitar WebView com API inativa, validar `BackendConfig.Url` antes de construir o grafo de serviços ou mostrar aviso nativo.

### `Frontend/MauiProgram.cs`

- **Problema:** `AddMauiBlazorWebView()` sem configuração adicional visível; em modo depuração ativa ferramentas de programador do Blazor WebView.
- **Por que causa tela branca:** Registo de serviços parece coerente (`RootComponent` está no XAML). Falha de **injeção de dependências** em tempo de execução num componente registado com **âmbito por componente** (`AddScoped`) **pode** derrubar o circuito Blazor após o primeiro render — **incerto** por página.
- **Como corrigir:** Usar o limitador de exceções do Blazor e registos; garantir que nenhum serviço com âmbito por componente injetado na primeira rota lança no construtor (ver `LoginViewModel` — construtor só regista comandos, baixo risco).

### `Frontend/wwwroot/index.html`

- **Problema:** `<base href="/" />`; ficheiro `_framework/blazor.webview.js` com `autostart="false"`; referência a `favicon.png` inexistente na pasta; texto da região `#blazor-error-ui` não está em português (não afeta branco).
- **Por que causa tela branca:** Se o caminho da página anfitriã ou dos estáticos estiver errado na publicação, o motor não arranca o Blazor e `#app` não é substituído.
- **Como corrigir:** Confirmar na pasta de saída a presença de `_framework` e de `blazor.webview.js`; adicionar `favicon.png` ou remover a linha do ícone; validar `base` se algum dia a aplicação for servida sob subcaminho (hoje não parece o caso).

### `Frontend/Components/Routes.razor`

- **Problema:** `Router` com `AppAssembly` apontando para o assembly do `MauiProgram`; `AuthorizeRouteView` com `Authorizing` mostrando texto — não é branco.
- **Por que causa tela branca:** Se o assembly estivesse errado, **não** haveria rotas — veria a vista `NotFound` (recurso não encontrado), não branco puro (a menos que estilos não carreguem).
- **Como corrigir:** Manter `AppAssembly` alinhado ao assembly que contém os `@page`.

### `Frontend/Components/Pages/Index.razor`

- **Problema:** `@page "/"` com `OnInitialized` que navega com **recarregamento forçado** para `/login` ou `/home`.
- **Por que causa tela branca:** Recarregar o documento completo no híbrido pode causar **intervalo** em que só se vê fundo ou conteúdo vazio; em casos extremos, comportamento inconsistente.
- **Como corrigir:** Preferir `forceLoad: false` ou `NavigationManager.NavigateTo` sem recarregar, salvo requisito comprovado.

### `Frontend/Components/RedirectToLogin.razor`

- **Problema:** Idem: `NavigateTo("/login", forceLoad: true)`.
- **Por que causa tela branca:** Mesmo mecanismo que em `Index.razor`.
- **Como corrigir:** Avaliar remoção de `forceLoad: true`.

### `Frontend/Frontend.csproj`

- **Problema:** `EnableDefaultCssItems` está `false` (gestão explícita de CSS); não há lista explícita de `wwwroot` no excerto — o projeto Razor/MAUI costuma incluir `wwwroot` automaticamente; alvos personalizados copiam apenas o **Backend**, não o `wwwroot`.
- **Por que causa tela branca:** Se a **cadeia de compilação** personalizada algum dia excluir estáticos do MAUI Blazor, faltariam ficheiros.
- **Como corrigir:** Após cada **publicação**, listar ficheiros ao lado do `.exe` e confirmar `wwwroot` e `_framework`.

### `Frontend/Services/AuthenticationStateService.cs` / `Frontend/Services/CustomAuthenticationStateProvider.cs`

- **Problema:** `GetUserClaimsAsync` lê `SecureStorage`; `HandleAuthenticationStateChanged` é `async void` (mau padrão para propagação de erros).
- **Por que causa tela branca:** Na primeira avaliação de autenticação, falhas raras de armazenamento seguro — **incerto, mas possível** exceção no fornecedor de estado — normalmente o Blazor mostraria erro, não branco eterno.
- **Como corrigir:** Tratar exceções em `GetAuthenticationStateAsync` e registar; evitar `async void` ou envolver em captura com registo.

### `Frontend/Platforms/Windows/App.xaml.cs` (anfitrião WinUI)

- **Problema:** Apenas delega `CreateMauiApp()` para `MauiProgram`.
- **Por que causa tela branca:** Nada específico; anfitrião padrão.
- **Como corrigir:** Não obrigatório salvo integração nativa extra.

---

## Falhas de arquitetura

- **Ausência de registo estruturado** quando o WebView2 ou o carregamento do `HostPage` falham (não há pontos de extensão no código atual).
- **Ausência de página nativa alternativa** quando o `BlazorWebView` não confirma carregamento do `index.html` ou do ficheiro de arranque JavaScript.
- **Dependência de servidor de rede de entrega de conteúdos** para um ficheiro de estilos em `index.html` sem cópia local — risco em redes isoladas (efeito visual, raramente branco total).
- **Mensagem de erro global do Blazor** (`#blazor-error-ui`) depende de CSS e de lógica para tornar visível; se o utilizador relata “só branco”, pode ser que **nem a cadeia de tratamento de erro** esteja a correr (falha antes do Blazor).

---

## Plano de correção

1. **Confirmar pré-requisito no Windows:** tempo de execução WebView2 instalado e não bloqueado; testar numa máquina ou utilizador sem políticas restritivas.
2. **Inspecionar saída da compilação/publicação:** pastas `wwwroot` e `_framework` presentes junto ao executável; abrir `index.html` no explorador **não** substitui o teste no WebView2, mas confirma ficheiros copiados.
3. **Alterar temporariamente** `BackgroundColor` em `MainPage.xaml` para distinguir falha nativa de falha do conteúdo web.
4. **Remover ou testar sem** `forceLoad: true` em `Index.razor` e `RedirectToLogin.razor`; validar se a tela branca desaparece.
5. **Adicionar diagnóstico:** subscrição a eventos do `BlazorWebView` (conforme documentação da versão 8.0.90) e registos na consola ou ficheiro.
6. **Corrigir `favicon.png`** ou remover a referência no HTML para eliminar pedidos falhados ao ícone.
7. **Depuração:** executar em modo depuração com ferramentas de programador do Blazor WebView ativas (`AddBlazorWebViewDeveloperTools` já condicionado a depuração em `MauiProgram.cs`) e inspecionar consola JavaScript e rede.
8. **Se persistir:** capturar registo do Windows (Visualizador de eventos) e verificar processos bloqueados relacionados com o motor de visualização embutido do Edge.

---

*Documento gerado com base na revisão do código do repositório CanilApp (Frontend MAUI Blazor, alvo Windows). O comportamento exato do motor WebView2 depende da versão instalada no sistema e não está totalmente determinável só pelo código-fonte.*
