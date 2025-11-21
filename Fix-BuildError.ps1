# Fix-BuildError.ps1
# Script para corrigir erro NETSDK1112 - Runtime Pack não encontrado

param(
    [string]$Configuration = "Release"
)

Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host "🔧 CORREÇÃO AUTOMÁTICA - ERRO NETSDK1112" -ForegroundColor Cyan
Write-Host "============================================================================" -ForegroundColor Cyan

$ErrorActionPreference = "Continue" # Continua mesmo com erros

# Verifica se está na raiz do projeto
if (!(Test-Path "CanilApp.sln")) {
    Write-Host "❌ ERRO: CanilApp.sln não encontrado!" -ForegroundColor Red
    Write-Host "   Execute este script na raiz do projeto onde está o .sln" -ForegroundColor Yellow
    exit 1
}

Write-Host "`n📍 Projeto encontrado!" -ForegroundColor Green
Write-Host "   Pasta: $(Get-Location)" -ForegroundColor Cyan

# ============================================================================
# ETAPA 1: VERIFICA SDK DO .NET
# ============================================================================
Write-Host "`n🔍 Verificando SDK do .NET..." -ForegroundColor Yellow

$sdkVersion = dotnet --version
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ SDK do .NET não encontrado!" -ForegroundColor Red
    Write-Host "   Baixe e instale: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ SDK do .NET instalado: $sdkVersion" -ForegroundColor Green

# Verifica se é versão 8.0+
if ($sdkVersion -notmatch "^8\.") {
    Write-Host "⚠️ AVISO: Versão do SDK é $sdkVersion (recomendado: 8.0.x)" -ForegroundColor Yellow
}

# ============================================================================
# ETAPA 2: REMOVE PASTAS BIN/OBJ
# ============================================================================
Write-Host "`n🧹 Removendo pastas bin/obj..." -ForegroundColor Yellow

$removedCount = 0
Get-ChildItem -Path . -Include bin,obj -Recurse -Directory -ErrorAction SilentlyContinue | ForEach-Object {
    try {
        Remove-Item $_.FullName -Recurse -Force -ErrorAction Stop
        $removedCount++
        Write-Host "   ✓ Removido: $($_.FullName)" -ForegroundColor Gray
    } catch {
        Write-Host "   ⚠️ Não foi possível remover: $($_.FullName)" -ForegroundColor Yellow
    }
}

Write-Host "✅ $removedCount pastas removidas" -ForegroundColor Green

# ============================================================================
# ETAPA 3: LIMPA CACHE DO NUGET (OPCIONAL)
# ============================================================================
Write-Host "`n🗑️ Limpando cache do NuGet..." -ForegroundColor Yellow

dotnet nuget locals all --clear | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Cache do NuGet limpo" -ForegroundColor Green
} else {
    Write-Host "⚠️ Não foi possível limpar cache (pode ser ignorado)" -ForegroundColor Yellow
}

# ============================================================================
# ETAPA 4: RESTAURA PACOTES COM WIN-X64
# ============================================================================
Write-Host "`n📦 Restaurando pacotes com RuntimeIdentifier win-x64..." -ForegroundColor Yellow

dotnet restore CanilApp.sln --runtime win-x64 --force

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ ERRO ao restaurar pacotes!" -ForegroundColor Red
    Write-Host "`n💡 SUGESTÕES:" -ForegroundColor Yellow
    Write-Host "   1. Verifique sua conexão com a internet" -ForegroundColor Gray
    Write-Host "   2. Tente: dotnet workload restore" -ForegroundColor Gray
    Write-Host "   3. Verifique se há erros de certificado SSL" -ForegroundColor Gray
    exit 1
}

Write-Host "✅ Pacotes restaurados com sucesso!" -ForegroundColor Green

# ============================================================================
# ETAPA 5: COMPILA BACKEND
# ============================================================================
Write-Host "`n🔨 Compilando Backend ($Configuration)..." -ForegroundColor Yellow

dotnet build Backend\Backend.csproj --configuration $Configuration --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ ERRO ao compilar Backend!" -ForegroundColor Red
    Write-Host "`n💡 Verifique os erros acima e corrija o código" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Backend compilado com sucesso!" -ForegroundColor Green

# ============================================================================
# ETAPA 6: COMPILA FRONTEND (APENAS WINDOWS)
# ============================================================================
Write-Host "`n🔨 Compilando Frontend ($Configuration) - Windows..." -ForegroundColor Yellow

dotnet build Frontend\Frontend.csproj `
    --configuration $Configuration `
    --framework net8.0-windows10.0.19041.0 `
    --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ ERRO ao compilar Frontend!" -ForegroundColor Red
    Write-Host "`n💡 Verifique os erros acima e corrija o código" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Frontend compilado com sucesso!" -ForegroundColor Green

# ============================================================================
# ETAPA 7: VALIDA SAÍDA
# ============================================================================
Write-Host "`n🔍 Validando arquivos de saída..." -ForegroundColor Yellow

$backendOutput = "Backend\bin\$Configuration\net8.0"
$frontendOutput = "Frontend\bin\$Configuration\net8.0-windows10.0.19041.0\win10-x64"

$backendExists = Test-Path "$backendOutput\Backend.dll"
$frontendExists = Test-Path "$frontendOutput\Frontend.dll"

if ($backendExists) {
    Write-Host "✅ Backend.dll encontrado" -ForegroundColor Green
} else {
    Write-Host "⚠️ Backend.dll NÃO encontrado!" -ForegroundColor Yellow
}

if ($frontendExists) {
    Write-Host "✅ Frontend.dll encontrado" -ForegroundColor Green
} else {
    Write-Host "⚠️ Frontend.dll NÃO encontrado!" -ForegroundColor Yellow
}

# ============================================================================
# RESUMO FINAL
# ============================================================================
Write-Host "`n============================================================================" -ForegroundColor Cyan
Write-Host "✅ CORREÇÃO CONCLUÍDA COM SUCESSO!" -ForegroundColor Green
Write-Host "============================================================================" -ForegroundColor Cyan

Write-Host "`n📂 Pastas de saída:" -ForegroundColor Yellow
Write-Host "   Backend:  $(Resolve-Path $backendOutput)" -ForegroundColor Cyan
Write-Host "   Frontend: $(Resolve-Path $frontendOutput)" -ForegroundColor Cyan

Write-Host "`n🚀 PRÓXIMOS PASSOS:" -ForegroundColor Yellow
Write-Host "   1. Execute o Frontend:" -ForegroundColor Gray
Write-Host "      cd `"$frontendOutput`"" -ForegroundColor Cyan
Write-Host "      .\Frontend.exe" -ForegroundColor Cyan
Write-Host ""
Write-Host "   2. Ou execute o Backend manualmente:" -ForegroundColor Gray
Write-Host "      cd `"$backendOutput`"" -ForegroundColor Cyan
Write-Host "      dotnet Backend.dll --urls http://127.0.0.1:0" -ForegroundColor Cyan

Write-Host "`n============================================================================" -ForegroundColor Cyan
Write-Host "🎉 Build finalizado!" -ForegroundColor Green
Write-Host "============================================================================" -ForegroundColor Cyan
