# Fluxo de trabalho com Git — CanilApp

Guia para desenvolvedores trabalharem com Git neste repositório (solução **CanilApp.sln**: `Backend`, `Frontend`, `Shared`). Comandos exemplificados em **PowerShell** ou **Git Bash**; adapte caminhos se necessário.

---

## 1. Estratégia de branches recomendada

O repositório **não define branches obrigatórias por configuração**; a sugestão abaixo é um modelo simples, compatível com equipes pequenas e médias.

| Branch | Propósito |
|--------|-----------|
| **`main`** | Código considerado **estável** e pronto para release (ou alinhado ao que está em produção). |
| **`develop`** *(opcional)* | Integração contínua do dia a dia; `main` só recebe merges quando há release ou hotfix estável. |
| **`feature/nome-curto`** | Nova funcionalidade ou refatoração maior (ex.: `feature/sync-estoque`). |
| **`fix/descricao`** ou **`hotfix/descricao`** | Correção de bug; `hotfix/*` costuma partir de `main` quando o bug está em produção. |

**Sem `develop`:** é válido usar só `main` + `feature/*` e abrir PR direto para `main`, desde que a equipe combine revisões e testes mínimos.

**Nomenclatura:** use minúsculas, hífens e nomes objetivos (`feature/login-cognito`, não `feature/branch1`).

---

## 2. Fluxo completo de desenvolvimento

### 2.1 Atualizar a cópia local antes de começar

```powershell
cd C:\caminho\para\CanipApp
git fetch origin
git checkout main
git pull origin main
```

(Se usarem `develop`, troque `main` por `develop`.)

### 2.2 Criar branch de trabalho

```powershell
git checkout -b feature/minha-alteracao
```

### 2.3 Desenvolver e commitar

Verifique o que mudou:

```powershell
git status
git diff
```

Adicionar ficheiros (evite `git add .` sem olhar a lista):

```powershell
git add Backend/Services/AlgumServico.cs
git add Frontend/ViewModels/AlgumViewModel.cs
```

Criar commit com mensagem clara (ver secção 3):

```powershell
git commit -m "feat(sync): melhora tratamento de erro na sincronização"
```

### 2.4 Atualizar a sua branch com a branch base

Enquanto trabalha, a `main` (ou `develop`) pode avançar. Traga essas mudanças para a sua branch para reduzir conflitos no PR.

**Opção A — merge (simples):**

```powershell
git fetch origin
git merge origin/main
```

**Opção B — rebase (histórico linear; use só se a equipa concordar):**

```powershell
git fetch origin
git rebase origin/main
```

Resolva conflitos se o Git indicar, teste o projeto (`dotnet build`, executar Frontend/Backend) e só depois continue.

### 2.5 Enviar a branch para o remoto

```powershell
git push -u origin feature/minha-alteracao
```

Na primeira vez, `-u` associa a branch local ao remoto; depois basta `git push`.

### 2.6 Abrir Pull Request (PR)

1. No GitHub (ou na plataforma que o time usar), abra um **Pull Request** da sua branch para `main` ou `develop`.
2. Preencha título e descrição: **o quê**, **porquê**, como testar (ex.: “login com user de teste Cognito”, “build da solução”).
3. Este repositório tem [`.github/CODEOWNERS`](.github/CODEOWNERS): em GitHub, revisores indicados podem ser solicitados automaticamente — respeite as revisões da equipa.
4. **Não há workflows de CI** definidos em `.github/workflows` neste projeto; a validação depende de revisão humana e testes locais.

Após aprovação, faça **merge** pela interface (merge commit, squash ou rebase — conforme política do time).

### 2.7 Depois do merge

```powershell
git checkout main
git pull origin main
git branch -d feature/minha-alteracao
```

---

## 3. Convenções de commit

Use mensagens que expliquem a **intenção** da mudança. Um formato amplamente usado é **Conventional Commits**:

```
<tipo>(escopo opcional): descrição curta no imperativo

Corpo opcional: detalhes, motivo, breaking changes.
```

### Tipos sugeridos

| Tipo | Quando usar |
|------|-------------|
| **feat** | Nova funcionalidade (API, ecrã, fluxo). |
| **fix** | Correção de bug. |
| **docs** | Apenas documentação (`README.md`, `docs/`, comentários de doc). |
| **chore** | Manutenção (formatar, scripts, `.gitignore` sem impacto funcional). |
| **refactor** | Reestruturação de código sem mudar comportamento. |
| **test** | Adicionar ou ajustar testes. |
| **perf** | Melhoria de desempenho. |

### Exemplos

```text
feat(backend): adiciona endpoint de health detalhado
fix(frontend): corrige navegação após login
docs: atualiza README com passos do SQLite
chore: alinha versão do pacote AWSSDK no Backend
refactor(sync): extrai helper de datas UTC
```

**Boas práticas:** uma ideia por commit quando possível; descrição na primeira linha até ~72 caracteres; não commits com mensagens vazias ou só “fix”.

---

## 4. Regras importantes — o que **não** commitar

### 4.1 Alinhado ao `.gitignore` deste repositório

O ficheiro [`.gitignore`](.gitignore) combina regras típicas de **Visual Studio / .NET** com entradas específicas do projeto. Em particular, **não deve** ir para o Git:

| Categoria | Exemplos / padrões |
|-----------|-------------------|
| **Segredos e ambiente** | `*.env` |
| **Configuração local do Backend com segredos** | `Backend/appsettings.json`, `Backend/appsettings.*.json` (padrão no `.gitignore`), `**/appsettings.Development.json` |
| **Launch profiles locais** | `Backend/Properties/launchSettings.json` |
| **Build e IDE** | `bin/`, `obj/`, `.vs/`, `.idea/`, ficheiros `*.user` |
| **Bases de dados locais** | `*.db`, `*.db-shm`, `*.db-wal`, e no projeto `Backend/canilapp.db` (entrada dedicada) |
| **Publicação / pacotes** | pastas de publish, `*.nupkg`, etc. (ver `.gitignore` completo) |

**Nota:** o backend usa SQLite em `%LocalAppData%\CanilApp\canilapp.db` em runtime; ficheiros `.db` na raiz de pastas do projeto também são ignorados pelo padrão global `*.db`.

### 4.2 O que **deve** ser versionado (comum neste projeto)

- Código-fonte `.cs`, `.razor`, `.xaml`, recursos MAUI, etc.
- **`Backend/Migrations/`** quando o modelo EF Core mudar (migrations são parte do histórico do schema).
- `CanilApp.sln`, `.csproj`, `docs/`, `README.md`, `GIT_WORKFLOW.md`, etc.
- Ficheiros de exemplo **sem segredos** (ex.: `appsettings.example.json`), **se** forem adicionados ao repositório e **não** estiverem cobertos por um padrão de ignore — hoje `Backend/appsettings.*.json` está ignorado; um nome como `appsettings.example.json` pode precisar de regra `!` no `.gitignore` se quiserem versioná-lo (avaliar com o time).

### 4.3 AWS, Cognito e credenciais

- **Nunca** commitar: chaves de acesso AWS, secrets de cliente Cognito além dos IDs públicos acordados, passwords, connection strings reais.
- O `Backend.csproj` define `UserSecretsId`: use **`dotnet user-secrets`** para desenvolvimento local (segredos ficam fora do repositório).
- IDs de User Pool / Client / Identity Pool podem ser partilhados por canal interno seguro; o repositório está configurado para **não** versionar `appsettings` do Backend por defeito.

### 4.4 Verificar antes do `git add`

```powershell
git status
git diff --staged
```

Se aparecer `appsettings.json`, `.env`, ou ficheiros sob `bin/`/`obj/`, **não** os inclua — se o Git os mostrar como “untracked”, o ignore pode estar incompleto ou o ficheiro está fora dos padrões; em caso de dúvida, pergunte ao time.

---

## 5. Boas práticas específicas — CanilApp (.NET, AWS, MAUI)

1. **Build antes do PR:** na raiz da solução, `dotnet build CanilApp.sln` (ou build no Visual Studio). O **Frontend** compila o **Backend** e copia artefactos; erros no Backend aparecem no fluxo normal de build.
2. **Migrations:** alterações ao `CanilAppDbContext` devem ir acompanhadas de novas migrations em `Backend/Migrations` e descrição no PR de como aplicar (`dotnet ef database update` no projeto Backend), conforme o [README](README.md).
3. **Configuração:** novos developers precisam de `appsettings` local ou User Secrets — documentado no README; não commite esses ficheiros.
4. **AWS:** mudanças que dependam de IAM, Cognito ou DynamoDB devem ser descritas no PR (permissões, fluxos) para quem revisa validar impacto.
5. **Windows / MAUI:** o alvo principal do Frontend é Windows; PRs que afetem `Platforms/Windows` ou `BackendStarter` merecem teste manual no ambiente Windows.
6. **Ficheiros grandes e binários:** evitar commitar builds publicados, `.exe` gerados ou dumps de BD; use releases ou artefactos fora do Git.
7. **CODEOWNERS:** respeitar revisões obrigatórias definidas em `.github/CODEOWNERS` ao abrir PRs no GitHub.

---

## Referências no repositório

| Ficheiro | Conteúdo relevante |
|----------|-------------------|
| [`.gitignore`](.gitignore) | Padrões de exclusão (inclui appsettings do Backend, `.env`, build, BD). |
| [`README.md`](README.md) | Setup local, segredos, SQLite, Cognito. |
| [`docs/DEPENDENCIAS.md`](docs/DEPENDENCIAS.md) | Versões de pacotes e frameworks. |
| [`.github/CODEOWNERS`](.github/CODEOWNERS) | Revisores padrão no GitHub. |

---

*Documento gerado com base na estrutura e ficheiros atuais do repositório. Ajuste nomes de branches e política de merge ao acordo da sua equipa.*
