# CSRF (CWE-352) Tester - GitHub Setup Script

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   CSRF (CWE-352) Tester - GitHub Setup" -ForegroundColor Cyan  
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if git is installed
try {
    $gitVersion = git --version
    Write-Host "✅ Git found: $gitVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Error: Git is not installed or not in PATH" -ForegroundColor Red
    Write-Host "Please install Git from: https://git-scm.com/" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "Step 1: Initializing Git repository..." -ForegroundColor Yellow
git init

Write-Host ""
Write-Host "Step 2: Adding all files to Git..." -ForegroundColor Yellow
git add .

Write-Host ""
Write-Host "Step 3: Creating initial commit..." -ForegroundColor Yellow
$commitMessage = @"
Initial commit: CSRF (CWE-352) vulnerability testing tool

- Complete C# console application using .NET 8
- Microsoft Edge browser automation with Playwright
- Comprehensive CSRF detection capabilities
- Command-line interface with multiple options
- Support for authenticated and unauthenticated testing
- JSON output for vulnerability reports
- Ready-to-use test scripts for safe testing sites
- Detailed documentation and usage examples
"@

git commit -m $commitMessage

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   NEXT STEPS:" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Go to GitHub.com and create a new repository" -ForegroundColor White
Write-Host "   Repository name: " -NoNewline -ForegroundColor White
Write-Host "csrf-cwe-352-tester" -ForegroundColor Green
Write-Host "   Description: " -NoNewline -ForegroundColor White
Write-Host "A C# tool for detecting CSRF vulnerabilities using Microsoft Edge automation" -ForegroundColor Green
Write-Host "   Make it public or private as desired" -ForegroundColor White
Write-Host "   DO NOT initialize with README (we already have one)" -ForegroundColor Yellow
Write-Host ""
Write-Host "2. Copy the repository URL (something like):" -ForegroundColor White
Write-Host "   https://github.com/YOUR_USERNAME/csrf-cwe-352-tester.git" -ForegroundColor Cyan
Write-Host ""
Write-Host "3. Run these commands with YOUR actual GitHub URL:" -ForegroundColor White
Write-Host ""
Write-Host "   git branch -M main" -ForegroundColor Green
Write-Host "   git remote add origin https://github.com/YOUR_USERNAME/csrf-cwe-352-tester.git" -ForegroundColor Green
Write-Host "   git push -u origin main" -ForegroundColor Green
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Repository initialized successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan

Read-Host "Press Enter to exit"
