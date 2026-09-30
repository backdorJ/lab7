@echo off
chcp 65001 >nul
cd /d "%~dp0src\TaskManager.Api"
echo Запуск API и веб-интерфейса: http://localhost:5080
dotnet run --launch-profile http
echo.
pause
