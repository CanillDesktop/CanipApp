@echo off
chcp 65001 > nul
echo Executando diagnostico...
powershell -ExecutionPolicy Bypass -File "diagnostico-simples.ps1"