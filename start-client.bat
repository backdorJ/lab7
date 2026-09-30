@echo off
chcp 65001 >nul
cd /d "%~dp0src\TaskManager.Client"
echo Запуск клиента Windows Forms
dotnet run
echo.
pause
