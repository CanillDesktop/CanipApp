# Fix-BuildError-V2.ps1
# Corrige erros NETSDK1112 e NETSDK1047 de build

param(
    [string]$Configuration = "Release"
)

Write-Host "=============================================================" -ForegroundColor Cyan
Write-Host "EXECUTANDO CORREÇÃO COMPLETA DE BUILD - CANILAPP" -ForegroundColor Cyan
Write-Host "=============================================================" -ForegroundColor Cyan

$ErrorActionPreference = "Stop"

# Verifica se está na raiz
if (!(Test-Path "CanilApp.sln")) {
    Write-Host "ERRO: Execute este script na raiz do projeto!" -ForegroundColor Red
    exit 1
}

Write-Host "Projeto encontrado em: $(Get-Location)" -ForegroundColor Green

# -----------------------------------------
# ETAPA 1 – LIMPEZA COMPLETA
# -----------------------------------------
Write-Host "Limpando bin/obj..." -ForegroundColor Yellow

Get-ChildItem -Include bin,obj -Recurse -Directory -ErrorAction SilentlyContinue |
    ForEach-Object {
        Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
    }

Write-Host "Pastas bin/obj removidas" -ForegroundColor Green

# -----------------------------------------
# ETAPA 2 – RESTAURA BACKEND
# -----------------------------------------
Write-Host "Restaurando Backend..." -ForegroundColor Yellow

dotnet restore Backend\Backend.csproj --runtime win-x64
if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao restaurar Backend." -ForegroundColor Red
    exit 1
}

Write-Host "Backend restaurado" -ForegroundColor Green

# -----------------------------------------
# ETAPA 3 – COMPILA BACKEND
# -----------------------------------------
Write-Host "Compilando Backend..." -ForegroundColor Yellow

dotnet build Backend\Backend.csproj --configuration $Configuration --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao compilar Backend." -ForegroundColor Red
    exit 1
}

Write-Host "Backend compilado" -ForegroundColor Green

# -----------------------------------------
# ETAPA 4 – RESTAURA FRONTEND
# -----------------------------------------
Write-Host "Restaurando Frontend..." -ForegroundColor Yellow

dotnet restore Frontend\Frontend.csproj -p:RuntimeIdentifier=win10-x64



if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao restaurar Frontend." -ForegroundColor Red
    exit 1
}

Write-Host "Frontend restaurado" -ForegroundColor Green

# -----------------------------------------
# ETAPA 5 – COMPILA FRONTEND
# -----------------------------------------
Write-Host "Compilando Frontend..." -ForegroundColor Yellow

dotnet build Frontend\Frontend.csproj `
    --configuration $Configuration `
    --framework net8.0-windows10.0.19041.0 `
    --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao compilar Frontend." -ForegroundColor Red
    exit 1
}

Write-Host "Frontend compilado" -ForegroundColor Green

# -----------------------------------------
# FINALIZAÇÃO
# -----------------------------------------
Write-Host "=============================================================" -ForegroundColor Cyan
Write-Host "BUILD CONCLUÍDO COM SUCESSO!" -ForegroundColor Green
Write-Host "=============================================================" -ForegroundColor Cyan
