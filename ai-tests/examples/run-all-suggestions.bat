@echo off
REM Batch runner for all suggestion tests
REM Usage: run-all-suggestions.bat <driver>
REM Example: run-all-suggestions.bat mock

if "%1"=="" (
    echo Usage: run-all-suggestions.bat ^<driver^>
    echo Available drivers: mock, ollama, openai
    exit /b 1
)

set DRIVER=%1
set BASE_URL=http://localhost:5000/api/test/ai
set SUGGESTIONS_DIR=..\suggestions

echo ========================================
echo Running All Suggestion Tests
echo Driver: %DRIVER%
echo ========================================
echo.

echo [1/4] Testing math lesson (fractions/decimals)...
curl -X POST "%BASE_URL%/%DRIVER%/suggestions" ^
     -H "Content-Type: application/json" ^
     -d @%SUGGESTIONS_DIR%\math-lesson.json ^
     -w "\nHTTP Status: %%{http_code}\nTime: %%{time_total}s\n" ^
     -s
echo.
echo ----------------------------------------
echo.

echo [2/4] Testing science lesson (photosynthesis VARK)...
curl -X POST "%BASE_URL%/%DRIVER%/suggestions" ^
     -H "Content-Type: application/json" ^
     -d @%SUGGESTIONS_DIR%\science-lesson.json ^
     -w "\nHTTP Status: %%{http_code}\nTime: %%{time_total}s\n" ^
     -s
echo.
echo ----------------------------------------
echo.

echo [3/4] Testing connections activity (water cycle)...
curl -X POST "%BASE_URL%/%DRIVER%/suggestions" ^
     -H "Content-Type: application/json" ^
     -d @%SUGGESTIONS_DIR%\connections-activity.json ^
     -w "\nHTTP Status: %%{http_code}\nTime: %%{time_total}s\n" ^
     -s
echo.
echo ----------------------------------------
echo.

echo [4/4] Testing VARK engagement (area/perimeter)...
curl -X POST "%BASE_URL%/%DRIVER%/suggestions" ^
     -H "Content-Type: application/json" ^
     -d @%SUGGESTIONS_DIR%\vark-engagement.json ^
     -w "\nHTTP Status: %%{http_code}\nTime: %%{time_total}s\n" ^
     -s
echo.
echo ========================================
echo All suggestion tests completed
echo ========================================
