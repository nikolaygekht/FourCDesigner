@echo off
REM Usage: test-validation.bat <driver> <json-file>
REM Example: test-validation.bat mock safe-input.json

if "%~1"=="" (
    echo Usage: test-validation.bat ^<driver^> ^<json-file^>
    echo.
    echo Drivers: mock, ollama, openai
    echo.
    echo Examples:
    echo   test-validation.bat mock safe-input.json
    echo   test-validation.bat ollama injection-attempt.json
    echo   test-validation.bat openai malicious-content.json
    exit /b 1
)

if "%~2"=="" (
    echo Usage: test-validation.bat ^<driver^> ^<json-file^>
    echo Example: test-validation.bat mock safe-input.json
    exit /b 1
)

set DRIVER=%~1
set JSON_FILE=%~2

echo ========================================
echo AI Validation Test
echo ========================================
echo Driver:  %DRIVER%
echo JSON:    %JSON_FILE%
echo Endpoint: POST /api/test/ai/validate/%DRIVER%
echo ========================================
echo.

curl -X POST "http://localhost:5000/api/test/ai/validate/%DRIVER%" ^
     -H "Content-Type: application/json" ^
     -d @"%JSON_FILE%" ^
     -w "\n\nStatus: %%{http_code}\nTime: %%{time_total}s\n"

echo.
