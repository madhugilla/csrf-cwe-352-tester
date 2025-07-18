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
- **Authentication Support** - Test authenticated and unauthenticated forms (including Azure Entra)
- **Azure Entra Integration** - Support for Blazor Server apps secured by Azure Entra (Azure AD)
- **Headless/GUI Mode** - Visual debugging or automated testing

## Usage

### Basic Usage

```bash
# Test a web page with forms
CSRFTester.exe --url https://example.com/contact-form

# Test with authentication
CSRFTester.exe --url https://example.com/admin/settings --username admin --password secret

# Test with Azure Entra authentication
CSRFTester.exe --url https://example.com/blazor-app --use-entra --entra-tenant-id your-tenant-id --entra-client-id your-client-id

# Test with Azure Entra and credentials for automated login
CSRFTester.exe --url https://example.com/blazor-app --use-entra --entra-tenant-id your-tenant-id --entra-client-id your-client-id --username user@company.com --password secret

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
- `--use-entra`: Use Azure Entra (Azure AD) authentication
- `--entra-tenant-id`: Azure Entra tenant ID (required when using --use-entra)
- `--entra-client-id`: Azure Entra client/application ID (required when using --use-entra)
- `--entra-redirect-uri`: Azure Entra redirect URI (optional, defaults to http://localhost)
- `--entra-scopes`: Azure Entra scopes (optional, space-separated)
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

## Azure Entra (Azure AD) Authentication

The tool supports testing Blazor Server applications secured by Azure Entra (formerly Azure AD). When using Azure Entra authentication, the tool will:

1. Navigate to your Blazor application URL
2. Detect the redirect to Azure AD login page (login.microsoftonline.com)
3. Handle the authentication flow automatically (if credentials provided) or wait for manual login
4. Wait for the redirect back to your application
5. Proceed with CSRF vulnerability testing on the authenticated session

### Azure Entra Usage Examples

```bash
# Test a Blazor Server app with Azure Entra (manual authentication)
CSRFTester.exe --url https://myapp.azurewebsites.net --use-entra --entra-tenant-id 12345678-1234-1234-1234-123456789012 --entra-client-id abcdefgh-1234-1234-1234-abcdefghijkl --headless false

# Test with automatic Azure Entra login
CSRFTester.exe --url https://myapp.azurewebsites.net --use-entra --entra-tenant-id 12345678-1234-1234-1234-123456789012 --entra-client-id abcdefgh-1234-1234-1234-abcdefghijkl --username user@company.com --password YourPassword123

# Test with custom redirect URI and scopes
CSRFTester.exe --url https://myapp.azurewebsites.net --use-entra --entra-tenant-id 12345678-1234-1234-1234-123456789012 --entra-client-id abcdefgh-1234-1234-1234-abcdefghijkl --entra-redirect-uri https://myapp.azurewebsites.net/signin-oidc --entra-scopes openid profile email
```

**Note**: When running in headless mode with Azure Entra authentication, you must provide username and password for automatic login. For manual authentication, use `--headless false` to see the browser window.

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
