# CSRF (CWE-352) Tester

A powerful C# console application for detecting Cross-Site Request Forgery (CSRF) vulnerabilities in web applications using Microsoft Edge browser automation.

## 🛡️ Overview

This tool automatically scans web applications for CSRF vulnerabilities by:
- Analyzing forms for missing or weak CSRF protection
- Testing token validation mechanisms
- Detecting state-changing operations without proper protection
- Providing detailed vulnerability reports

## ⚡ Features

- **Microsoft Edge Browser Automation** - Uses Playwright with Edge for reliable testing
- **Comprehensive CSRF Detection** - Multiple attack vectors and protection mechanisms
- **Command Line Interface** - Easy integration into CI/CD pipelines
- **Detailed Reporting** - JSON output with vulnerability details
- **Authentication Support** - Test authenticated and unauthenticated forms
- **Headless/GUI Mode** - Visual debugging or automated testing

## Usage

### Basic Usage

```bash
# Test a web page with forms
CSRFTester.exe --url https://example.com/contact-form

# Test with authentication
CSRFTester.exe --url https://example.com/admin/settings --username admin --password secret

# Run in headed mode (visible browser)
CSRFTester.exe --url https://example.com/forms --headless false

# Enable verbose logging
CSRFTester.exe --url https://example.com/forms --verbose

# Save detailed report to file
CSRFTester.exe --url https://example.com/forms --output report.json
```

### Command Line Options

- `--url` (required): Target web page URL to test
- `--username`: Username for form-based authentication
- `--password`: Password for form-based authentication
- `--headless`: Run browser in headless mode (default: true)
- `--verbose`: Enable verbose logging
- `--output`: Output file path for detailed JSON report

### Example Output

```
================================================================================
CSRF VULNERABILITY TEST RESULTS
================================================================================

Target URL: https://example.com/contact-form
Test Date: 2025-07-18 10:30:45 UTC

TEST RESULTS:
----------------------------------------
✓ PASS   [LOW]     Page Accessibility
✓ PASS   [LOW]     CSRF Token Presence (Form 1)
✗ FAIL   [HIGH]    Missing CSRF Token Test (Form 1)
✗ FAIL   [HIGH]    Invalid CSRF Token Test (Form 1)
✓ PASS   [LOW]     Origin Header Test (Form 1)

OVERALL ASSESSMENT:
----------------------------------------
🚨 VULNERABLE
Summary: ENDPOINT IS VULNERABLE TO CSRF ATTACKS (CWE-352) - 2 vulnerabilities found

RECOMMENDATIONS:
----------------------------------------
• Implement CSRF token validation for state-changing operations
• Use SameSite cookie attributes (SameSite=Strict or SameSite=Lax)
• Validate Origin and Referer headers
• Consider using double-submit cookie pattern
• Implement proper session management

================================================================================
```

## Prerequisites

Before running the application, you need to install Playwright browsers (Microsoft Edge):

```bash
# Install Playwright browsers (including Microsoft Edge)
pwsh bin/Debug/net8.0/playwright.ps1 install msedge

# Or on Linux/macOS
./bin/Debug/net8.0/playwright.sh install msedge
```

## Building

```bash
# Restore dependencies
dotnet restore

# Build the application
dotnet build

# Run the application
dotnet run -- --url https://example.com/contact-form
```

## Publishing

```bash
# Publish for Windows
dotnet publish -c Release -r win-x64 --self-contained

# Publish for Linux
dotnet publish -c Release -r linux-x64 --self-contained

# Publish for macOS
dotnet publish -c Release -r osx-x64 --self-contained
```

## Security Testing Methodology

The application performs the following tests:

1. **Endpoint Accessibility**: Verifies the target endpoint is reachable
2. **Authentication**: Attempts to authenticate if credentials are provided
3. **Missing CSRF Token**: Tests if requests without CSRF tokens are accepted
4. **Invalid CSRF Token**: Tests if requests with invalid tokens are accepted
5. **Token Reuse**: Tests if CSRF tokens can be reused multiple times
6. **Origin Header Validation**: Tests if malicious Origin headers are accepted
7. **Referer Header Validation**: Tests if malicious Referer headers are accepted

## Configuration

The application can be configured using `appsettings.json`:

```json
{
  "HttpClient": {
    "TimeoutSeconds": 60,
    "UserAgent": "CSRFTester/1.0"
  },
  "Testing": {
    "DefaultTestTimeoutSeconds": 30,
    "EnableDetailedLogging": false
  }
}
```

## Requirements

- .NET 8.0 or later
- Network access to target endpoints
- Optional: Authentication credentials for protected endpoints

## Disclaimer

This tool is intended for security testing purposes only. Use it responsibly and only on systems you own or have explicit permission to test. The authors are not responsible for any misuse of this tool.
