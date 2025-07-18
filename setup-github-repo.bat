@echo off
echo ========================================
echo    CSRF (CWE-352) Tester - GitHub Setup
echo ========================================
echo.

REM Check if git is installed
git --version >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo Error: Git is not installed or not in PATH
    echo Please install Git from: https://git-scm.com/
    pause
    exit /b 1
)

echo Step 1: Initializing Git repository...
git init

echo.
echo Step 2: Adding all files to Git...
git add .

echo.
echo Step 3: Creating initial commit...
git commit -m "Initial commit: CSRF (CWE-352) vulnerability testing tool

- Complete C# console application using .NET 8
- Microsoft Edge browser automation with Playwright
- Comprehensive CSRF detection capabilities
- Command-line interface with multiple options
- Support for authenticated and unauthenticated testing
- JSON output for vulnerability reports
- Ready-to-use test scripts for safe testing sites
- Detailed documentation and usage examples"

echo.
echo ========================================
echo    NEXT STEPS:
echo ========================================
echo.
echo 1. Go to GitHub.com and create a new repository
echo    Repository name: csrf-cwe-352-tester
echo    Description: A C# tool for detecting CSRF vulnerabilities using Microsoft Edge automation
echo    Make it public or private as desired
echo    DO NOT initialize with README (we already have one)
echo.
echo 2. Copy the repository URL (something like):
echo    https://github.com/YOUR_USERNAME/csrf-cwe-352-tester.git
echo.
echo 3. Run these commands with YOUR actual GitHub URL:
echo.
echo    git branch -M main
echo    git remote add origin https://github.com/YOUR_USERNAME/csrf-cwe-352-tester.git
echo    git push -u origin main
echo.
echo ========================================
echo Repository initialized successfully!
echo ========================================

pause
