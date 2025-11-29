Write-Host "=== DIAGNOSTICO DO BANCO DE DADOS ===" -ForegroundColor Cyan

# 1. Verificar banco de dados
$dbPath = "$env:LOCALAPPDATA\CanilApp\canilapp.db"
Write-Host "`n1. VERIFICANDO BANCO DE DADOS..." -ForegroundColor Yellow

if (Test-Path $dbPath) {
    Write-Host "   ✅ BANCO ENCONTRADO" -ForegroundColor Green
    $dbSize = (Get-Item $dbPath).Length
    Write-Host "   Tamanho: $([math]::Round($dbSize/1KB, 2)) KB" -ForegroundColor Gray
} else {
    Write-Host "   ❌ BANCO NAO ENCONTRADO" -ForegroundColor Red
    Write-Host "   Local: $dbPath" -ForegroundColor Gray
}

# 2. Verificar migrations
Write-Host "`n2. VERIFICANDO MIGRATIONS..." -ForegroundColor Yellow
$migrationsPath = "Backend\Migrations"

if (Test-Path $migrationsPath) {
    $migrations = Get-ChildItem $migrationsPath -Filter "*.cs"
    Write-Host "   ✅ PASTA ENCONTRADA" -ForegroundColor Green
    Write-Host "   Quantidade: $($migrations.Count) migrations" -ForegroundColor Gray
} else {
    Write-Host "   ❌ PASTA NAO ENCONTRADA" -ForegroundColor Red
}

# 3. Verificar logs
Write-Host "`n3. VERIFICANDO LOGS..." -ForegroundColor Yellow
$logsPath = "$env:LOCALAPPDATA\CanilApp\logs"

if (Test-Path $logsPath) {
    $logs = Get-ChildItem $logsPath -Filter "*.log"
    Write-Host "   ✅ PASTA ENCONTRADA" -ForegroundColor Green
    Write-Host "   Quantidade: $($logs.Count) arquivos de log" -ForegroundColor Gray
    
    if ($logs.Count -gt 0) {
        $latestLog = $logs | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        Write-Host "   Log mais recente: $($latestLog.Name)" -ForegroundColor Gray
    }
} else {
    Write-Host "   ❌ PASTA NAO ENCONTRADA" -ForegroundColor Red
}

Write-Host "`n=== DIAGNOSTICO CONCLUIDO ===" -ForegroundColor Green
Write-Host "Pressione qualquer tecla para sair..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")