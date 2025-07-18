@echo off
echo Building CSRF Tester...
dotnet build src/CSRFTester.csproj

if %ERRORLEVEL% neq 0 (
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo Build successful! Now testing against Altoro Mutual bank login...
echo.

dotnet run --project src/CSRFTester.csproj -- --url https://demo.testfire.net/login.jsp --headless false --verbose

pause
