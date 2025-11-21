@echo off
echo ========================================
echo COPIANDO BACKEND MANUALMENTE
echo ========================================

set SOURCE=Backend\bin\Debug\net8.0\win-x64
set DEST=Frontend\bin\Debug\net8.0-windows10.0.19041.0\win10-x64

echo.
echo Origem: %SOURCE%
echo Destino: %DEST%
echo.

if not exist "%SOURCE%\Backend.dll" (
    echo ❌ ERRO: Backend nao encontrado em %SOURCE%
    pause
    exit /b 1
)

if not exist "%DEST%" (
    echo 📁 Criando pasta de destino...
    mkdir "%DEST%"
)

echo 🚀 Copiando arquivos...
xcopy "%SOURCE%\*.*" "%DEST%\" /Y /E /I

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ✅ Backend copiado com sucesso!
    echo.
    dir "%DEST%\Backend.*"
) else (
    echo.
    echo ❌ ERRO ao copiar backend!
)

echo.
echo ========================================
pause
