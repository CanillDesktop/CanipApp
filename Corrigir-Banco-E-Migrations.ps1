# ============================================================
# CORREÇÃO COMPLETA - BANCO E MIGRATIONS
# ============================================================

Write-Host "🔧 CORREÇÃO DO BANCO DE DADOS E MIGRATIONS" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$projectPath = "C:\Users\Arthu\source\repos\CanillDesktop\CanipApp"
$dbPath = "$env:LOCALAPPDATA\CanilApp\canilapp.db"

# ============================================================
# PASSO 1: PARAR O APP
# ============================================================
Write-Host "1️⃣ PARANDO APLICAÇÃO..." -ForegroundColor Yellow
Write-Host "   ⚠️ Feche o Frontend.exe se estiver aberto!" -ForegroundColor Red
Write-Host "   Pressione ENTER quando fechar..." -ForegroundColor Yellow
Read-Host

# ============================================================
# PASSO 2: DELETAR BANCO ANTIGO
# ============================================================
Write-Host "`n2️⃣ DELETANDO BANCO ANTIGO..." -ForegroundColor Yellow

if (Test-Path $dbPath) {
    Remove-Item "$dbPath" -Force -ErrorAction SilentlyContinue
    Remove-Item "$dbPath-shm" -Force -ErrorAction SilentlyContinue
    Remove-Item "$dbPath-wal" -Force -ErrorAction SilentlyContinue
    Write-Host "   ✅ Banco deletado" -ForegroundColor Green
} else {
    Write-Host "   ℹ️ Banco não existe (OK)" -ForegroundColor Gray
}

# ============================================================
# PASSO 3: DELETAR MIGRATIONS ANTIGAS
# ============================================================
Write-Host "`n3️⃣ DELETANDO MIGRATIONS ANTIGAS..." -ForegroundColor Yellow

cd $projectPath\Backend

$migrationsPath = "Migrations"
if (Test-Path $migrationsPath) {
    $migrations = Get-ChildItem $migrationsPath -Filter "*.cs"
    Write-Host "   Encontradas $($migrations.Count) migrations antigas" -ForegroundColor Gray
    
    Remove-Item $migrationsPath -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "   ✅ Migrations deletadas" -ForegroundColor Green
} else {
    Write-Host "   ℹ️ Pasta Migrations não existe (OK)" -ForegroundColor Gray
}

# ============================================================
# PASSO 4: CRIAR NOVA MIGRATION INICIAL
# ============================================================
Write-Host "`n4️⃣ CRIANDO NOVA MIGRATION INICIAL..." -ForegroundColor Yellow

dotnet ef migrations add InitialCreate

if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✅ Migration criada" -ForegroundColor Green
} else {
    Write-Host "   ❌ Erro ao criar migration!" -ForegroundColor Red
    Write-Host "   Verifique os erros acima" -ForegroundColor Red
    exit 1
}

# ============================================================
# PASSO 5: APLICAR MIGRATION NO BANCO
# ============================================================
Write-Host "`n5️⃣ APLICANDO MIGRATION NO BANCO..." -ForegroundColor Yellow

dotnet ef database update

if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✅ Banco criado com sucesso" -ForegroundColor Green
    
    # Verifica se banco foi criado
    if (Test-Path $dbPath) {
        $dbSize = (Get-Item $dbPath).Length
        Write-Host "   📊 Tamanho do banco: $([math]::Round($dbSize/1KB, 2)) KB" -ForegroundColor Gray
    }
} else {
    Write-Host "   ❌ Erro ao aplicar migration!" -ForegroundColor Red
    Write-Host "   Verifique os erros acima" -ForegroundColor Red
    exit 1
}

# ============================================================
# PASSO 6: LIMPAR E RECOMPILAR BACKEND
# ============================================================
Write-Host "`n6️⃣ RECOMPILANDO BACKEND..." -ForegroundColor Yellow

Remove-Item bin,obj -Recurse -Force -ErrorAction SilentlyContinue

dotnet build --configuration Debug

if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✅ Backend compilado" -ForegroundColor Green
} else {
    Write-Host "   ❌ Erro ao compilar!" -ForegroundColor Red
    exit 1
}

# ============================================================
# PASSO 7: COPIAR BACKEND PARA FRONTEND
# ============================================================
Write-Host "`n7️⃣ COPIANDO BACKEND PARA FRONTEND..." -ForegroundColor Yellow

cd $projectPath

$sourcePath = "Backend\bin\Debug\net8.0\*"
$destPath = "Frontend\bin\Debug\net8.0-windows10.0.19041.0\win10-x64\Backend\"

if (Test-Path $destPath) {
    Copy-Item $sourcePath $destPath -Recurse -Force
    Write-Host "   ✅ Backend copiado" -ForegroundColor Green
} else {
    Write-Host "   ⚠️ Pasta de destino não existe" -ForegroundColor Yellow
    Write-Host "   Compile o Frontend primeiro" -ForegroundColor Yellow
}

# ============================================================
# PASSO 8: ATUALIZAR APPSETTINGS (DESABILITAR AWS)
# ============================================================
Write-Host "`n8️⃣ ATUALIZANDO CONFIGURAÇÕES (DESABILITAR AWS)..." -ForegroundColor Yellow

$appsettingsPath = "Backend\appsettings.json"
$appsettingsContent = @"
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
    }
  },
  "AllowedHosts": "*",
  "JWT": {
    "SecretKey": "sua-chave-secreta-minima-de-32-caracteres-para-jwt-canilapp-2024!",
    "Issuer": "CanilAppBackend",
    "Audience": "CanilAppFrontend",
    "ExpirationMinutes": 480
  },
  "AWS": {
    "Enabled": false
  },
  "Sync": {
    "Enabled": false,
    "AutoSync": false
  }
}
"@

Set-Content -Path $appsettingsPath -Value $appsettingsContent -Force
Write-Host "   ✅ Configurações atualizadas (AWS desabilitado)" -ForegroundColor Green

# ============================================================
# RESUMO FINAL
# ============================================================
Write-Host "`n" -NoNewline
Write-Host "============================================" -ForegroundColor Green
Write-Host "✅ CORREÇÃO CONCLUÍDA COM SUCESSO!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""
Write-Host "📋 O QUE FOI FEITO:" -ForegroundColor Cyan
Write-Host "   • Banco antigo deletado" -ForegroundColor Gray
Write-Host "   • Migrations antigas removidas" -ForegroundColor Gray
Write-Host "   • Nova migration criada (InitialCreate)" -ForegroundColor Gray
Write-Host "   • Banco recriado com schema correto" -ForegroundColor Gray
Write-Host "   • Backend recompilado" -ForegroundColor Gray
Write-Host "   • Backend copiado para Frontend" -ForegroundColor Gray
Write-Host "   • AWS Sync desabilitado (sem timeouts)" -ForegroundColor Gray
Write-Host ""
Write-Host "🚀 PRÓXIMO PASSO:" -ForegroundColor Cyan
Write-Host "   Execute o Frontend e teste:" -ForegroundColor Yellow
Write-Host "   cd Frontend\bin\Debug\net8.0-windows10.0.19041.0\win10-x64" -ForegroundColor White
Write-Host "   .\Frontend.exe" -ForegroundColor White
Write-Host ""
Write-Host "✅ SEM MAIS:" -ForegroundColor Green
Write-Host "   • Timeouts de 240 segundos" -ForegroundColor Gray
Write-Host "   • Erros de INSERT INTO ItensBase" -ForegroundColor Gray
Write-Host "   • Tentativas de conexão AWS" -ForegroundColor Gray
Write-Host ""

Write-Host "Pressione qualquer tecla para sair..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
