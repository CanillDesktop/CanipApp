@echo off
echo ========================================
echo DIAGNOSTICO DO CANILLDESKTOP
echo ========================================
echo.

echo [1] Verificando Backend compilado...
if exist "Backend\bin\Debug\net8.0\win-x64\Backend.dll" (
    echo     ✅ Backend.dll encontrado!
    dir "Backend\bin\Debug\net8.0\win-x64\Backend.*"
) else (
    echo     ❌ Backend.dll NAO encontrado!
)
echo.

echo [2] Verificando pasta de saida do Frontend...
if exist "Frontend\bin\Debug\net8.0-windows10.0.19041.0\" (
    echo     ✅ Pasta de saida existe!
    dir "Frontend\bin\Debug\net8.0-windows10.0.19041.0\" /ad
) else (
    echo     ❌ Pasta de saida NAO existe!
)
echo.

echo [3] Verificando se Backend foi copiado (win10-x64)...
if exist "Frontend\bin\Debug\net8.0-windows10.0.19041.0\win10-x64\Backend.dll" (
    echo     ✅ Backend copiado para win10-x64!
    dir "Frontend\bin\Debug\net8.0-windows10.0.19041.0\win10-x64\Backend.*"
) else (
    echo     ❌ Backend NAO copiado para win10-x64!
)
echo.

echo [4] Verificando se Backend foi copiado (win-x64)...
if exist "Frontend\bin\Debug\net8.0-windows10.0.19041.0\win-x64\Backend.dll" (
    echo     ✅ Backend copiado para win-x64!
    dir "Frontend\bin\Debug\net8.0-windows10.0.19041.0\win-x64\Backend.*"
) else (
    echo     ❌ Backend NAO copiado para win-x64!
)
echo.

echo [5] Listando TODOS os arquivos DLL no Frontend...
dir "Frontend\bin\Debug\net8.0-windows10.0.19041.0\*.dll" /s /b | findstr /i backend
echo.

echo ========================================
echo FIM DO DIAGNOSTICO
echo ========================================
pause
