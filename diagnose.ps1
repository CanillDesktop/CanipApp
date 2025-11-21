# diagnose.ps1
# Script de diagnóstico para CanilApp

Write-Host "════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "     DIAGNÓSTICO CANILAPP - AUTENTICAÇÃO        " -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# 1. Verifica .NET SDK
Write-Host "1. Verificando .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "   ✓ .NET SDK: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "   ✗ .NET SDK não encontrado!" -ForegroundColor Red
    Write-Host "   Instale em: https://dotnet.microsoft.com/download" -ForegroundColor Gray
}

# 2. Verifica estrutura de pastas
Write-Host "`n2. Verificando estrutura do projeto..." -ForegroundColor Yellow
$requiredFolders = @("Backend", "Frontend", "Shared")
foreach ($folder in $requiredFolders) {
    if (Test-Path $folder) {
        Write-Host "   ✓ Pasta $folder encontrada" -ForegroundColor Green
    } else {
        Write-Host "   ✗ Pasta $folder NÃO encontrada!" -ForegroundColor Red
    }
}

# 3. Verifica projetos
Write-Host "`n3. Verificando arquivos de projeto..." -ForegroundColor Yellow
$projects = @(
    "Backend\Backend.csproj",
    "Frontend\Frontend.csproj",
    "Shared\Shared.csproj"
)
foreach ($proj in $projects) {
    if (Test-Path $proj) {
        Write-Host "   ✓ $proj encontrado" -ForegroundColor Green
    } else {
        Write-Host "   ✗ $proj NÃO encontrado!" -ForegroundColor Red
    }
}

# 4. Verifica banco de dados
Write-Host "`n4. Verificando banco de dados..." -ForegroundColor Yellow
$dbPath = "Backend\canilapp.db"
if (Test-Path $dbPath) {
    $size = [math]::Round((Get-Item $dbPath).Length / 1KB, 2)
    Write-Host "   ✓ Banco encontrado: $dbPath (${size}KB)" -ForegroundColor Green

    try {
        Push-Location Backend
        $tables = sqlite3 canilapp.db ".tables" 2>$null
        if ($tables) {
            Write-Host "   ✓ Tabelas no banco: $tables" -ForegroundColor Green
        }
        Pop-Location
    } catch {
        Write-Host "   ! SQLite CLI não disponível para verificar tabelas" -ForegroundColor Gray
    }
} else {
    Write-Host "   ✗ Banco de dados NÃO encontrado!" -ForegroundColor Red
    Write-Host "   Execute: dotnet ef database update" -ForegroundColor Gray
}

# 5. Testa compilação
Write-Host "`n5. Testando compilação..." -ForegroundColor Yellow
$buildSuccess = $true

Write-Host "   Compilando Shared..." -ForegroundColor Gray
$result = dotnet build Shared\Shared.csproj --nologo --verbosity quiet 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✓ Shared compilado" -ForegroundColor Green
} else {
    Write-Host "   ✗ Erro ao compilar Shared" -ForegroundColor Red
    $buildSuccess = $false
}

Write-Host "   Compilando Backend..." -ForegroundColor Gray
$result = dotnet build Backend\Backend.csproj --nologo --verbosity quiet 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✓ Backend compilado" -ForegroundColor Green
} else {
    Write-Host "   ✗ Erro ao compilar Backend" -ForegroundColor Red
    $buildSuccess = $false
}

Write-Host "   Compilando Frontend..." -ForegroundColor Gray
$result = dotnet build Frontend\Frontend.csproj --nologo --verbosity quiet 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✓ Frontend compilado" -ForegroundColor Green
} else {
    Write-Host "   ✗ Erro ao compilar Frontend" -ForegroundColor Red
    $buildSuccess = $false
}

# 6. Verifica Backend copiado
Write-Host "`n6. Verificando Backend no Frontend..." -ForegroundColor Yellow
$backendInFrontend = "Frontend\bin\Debug\net8.0-windows10.0.19041.0\win10-x64\Backend\Backend.dll"
if (Test-Path $backendInFrontend) {
    Write-Host "   ✓ Backend.dll encontrado no Frontend" -ForegroundColor Green
} else {
    Write-Host "   ✗ Backend.dll NÃO encontrado no Frontend" -ForegroundColor Red
}

# 7. Testa Backend isoladamente
Write-Host "`n7. Testando Backend..." -ForegroundColor Yellow
Write-Host "   Iniciando Backend em modo teste..." -ForegroundColor Gray

$backendProcess = Start-Process -FilePath "dotnet" `
    -ArgumentList "run --project Backend\Backend.csproj --urls http://localhost:5555" `
    -NoNewWindow -PassThru `
    -RedirectStandardOutput "backend-output.txt" `
    -RedirectStandardError "backend-error.txt"

Start-Sleep -Seconds 5

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5555/api/health" -TimeoutSec 5
    Write-Host "   ✓ Backend respondendo corretamente" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Backend não está respondendo!" -ForegroundColor Red
    Write-Host "   Erro: $_" -ForegroundColor Gray
    
    if (Test-Path "backend-error.txt") {
        $errors = Get-Content "backend-error.txt" -Tail 10
        if ($errors) {
            Write-Host "   Últimos erros do Backend:" -ForegroundColor Red
            $errors | ForEach-Object { Write-Host "     $_" -ForegroundColor Gray }
        }
    }
}

Stop-Process -Id $backendProcess.Id -Force -ErrorAction SilentlyContinue

# 8. Verifica arquivos críticos
Write-Host "`n8. Verificando arquivos críticos..." -ForegroundColor Yellow
$criticalFiles = @{
    "Backend\Controllers\LoginController.cs" = "LoginController"
    "Backend\Controllers\UsuariosController.cs" = "UsuariosController"
    "Frontend\ViewModels\LoginViewModel.cs"   = "LoginViewModel"
    "Frontend\Services\BackendStarter.cs"     = "BackendStarter"
    "Frontend\Handlers\AuthDelegatingHandler.cs" = "AuthDelegatingHandler"
}

foreach ($file in $criticalFiles.Keys) {
    if (Test-Path $file) {
        Write-Host "   ✓ $($criticalFiles[$file]) encontrado" -ForegroundColor Green
    } else {
        Write-Host "   ✗ $($criticalFiles[$file]) NÃO encontrado!" -ForegroundColor Red
    }
}

# Resumo
Write-Host "`n════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "                    RESUMO                      " -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════" -ForegroundColor Cyan

if ($buildSuccess) {
    Write-Host "`n✓ Projeto compila corretamente" -ForegroundColor Green
} else {
    Write-Host "`n✗ Há erros de compilação" -ForegroundColor Red
}

# Recomendações
Write-Host "`nRECOMENDAÇÕES:" -ForegroundColor Yellow
Write-Host @"

1. Se o banco não existe:
   cd Backend
   dotnet ef migrations add InitialCreate
   dotnet ef database update

2. Para recriar tudo:
   .\reset-and-build.ps1

3. Para testar o Backend:
   cd Backend
   dotnet run
   
4. Para testar o Frontend:
   cd Frontend
   dotnet run

5. Verifique os logs em:
   - backend-output.txt
   - backend-error.txt
   
"@ -ForegroundColor Gray

Remove-Item "backend-output.txt" -ErrorAction SilentlyContinue
Remove-Item "backend-error.txt" -ErrorAction SilentlyContinue

Write-Host "Diagnóstico concluído!" -ForegroundColor Green
