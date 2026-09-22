@echo off
title Recruitment System Web API Server
cls
echo =================================================================
echo        KHOI DONG HE THONG RECRUITMENT SYSTEM WEB API
echo =================================================================
echo  [+] Web API:  http://localhost:5000
echo  [+] Swagger:  http://localhost:5000/swagger
echo =================================================================
echo.
echo Dang mo Trinh duyet den trang Swagger UI...
timeout /t 2 /nobreak >nul
start http://localhost:5000/swagger
echo Dang chay Server... (Nhan Ctrl+C de dung Server)
echo.
dotnet run
pause
