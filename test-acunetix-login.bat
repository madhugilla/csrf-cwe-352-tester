@echo off
echo Building CSRF Tester...
dotnet build src/CSRFTester.csproj

if %ERRORLEVEL% neq 0 (
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo Build successful! Now testing against Acunetix login form...
echo.

dotnet run --project src/CSRFTester.csproj -- --url http://testphp.vulnweb.com/login.php --headless false --verbose

pause
