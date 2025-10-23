@echo off
REM Usage: test-suggestions.bat <driver> <json-file>
REM Example: test-suggestions.bat mock math-lesson.json

if "%~1"=="" (
    echo Usage: test-suggestions.bat ^<driver^> ^<json-file^>
    echo.
    echo Drivers: mock, ollama, openai
    echo.
    echo Examples:
    echo   test-suggestions.bat mock math-lesson.json
    echo   test-suggestions.bat ollama science-lesson.json
    echo   test-suggestions.bat openai connections-activity.json
    exit /b 1
)

if "%~2"=="" (
    echo Usage: test-suggestions.bat ^<driver^> ^<json-file^>
    echo Example: test-suggestions.bat mock math-lesson.json
    exit /b 1
)

set DRIVER=%~1
set JSON_FILE=%~2

echo ========================================
echo AI Suggestions Test
echo ========================================
echo Driver:  %DRIVER%
echo JSON:    %JSON_FILE%
echo Endpoint: POST /api/test/ai/suggestions/%DRIVER%
echo ========================================
echo.

curl -X POST "http://localhost:5000/api/test/ai/suggestions/%DRIVER%" ^
     -H "Content-Type: application/json" ^
     -d @"%JSON_FILE%" ^
     -w "\n\nStatus: %%{http_code}\nTime: %%{time_total}s\n"

echo.
