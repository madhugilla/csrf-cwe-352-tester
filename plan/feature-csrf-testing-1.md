---
goal: Implement Console Application for CSRF (CWE-352) UI Vulnerability Testing
version: 1.0
date_created: 2025-07-18
last_updated: 2025-07-18
owner: Security Testing Team
tags: [feature, security, testing, csrf, cwe-352, ui-testing, playwright]
---

# Introduction

This implementation plan establishes a console application for testing web user interfaces (UI) for Cross-Site Request Forgery (CSRF) vulnerabilities (CWE-352). The application will accept a target web page URL as input, interact with forms and UI elements using browser automation, perform automated CSRF vulnerability detection on form submissions, and provide clear results indicating whether the web application is vulnerable to CSRF attacks.

## 1. Requirements & Constraints

- **REQ-001**: Console application must accept web page URL as command-line argument or interactive input
- **REQ-002**: Application must use browser automation to interact with web forms and UI elements
- **REQ-003**: Application must detect CSRF vulnerabilities in form submissions and state-changing UI actions
- **REQ-004**: Tests must validate CSRF token presence in forms, format, and server-side validation
- **REQ-005**: Application must test forms for missing CSRF tokens, invalid tokens, and token reuse scenarios
- **REQ-006**: Application must provide clear, actionable vulnerability reports in console output
- **REQ-007**: Support for form-based authentication and login flows
- **REQ-008**: Application must handle JavaScript-rendered forms and AJAX submissions
- **REQ-009**: Support for Azure Entra (Azure AD) authentication for Blazor Server applications
- **REQ-010**: Application must handle OAuth 2.0/OpenID Connect authentication flows
- **SEC-001**: Application must not perform destructive operations on target systems (read-only testing)
- **SEC-002**: All browser interactions must be logged for audit purposes
- **SEC-003**: Test credentials must be configurable and not hardcoded
- **CON-001**: Application must run on Windows, Linux, and macOS (.NET cross-platform with Playwright)
- **CON-002**: Each test must complete within 60 seconds to avoid timeouts
- **CON-003**: Application must handle browser errors and network issues gracefully
- **GUD-001**: Follow OWASP testing guidelines for CSRF detection in web applications
- **GUD-002**: Provide verbose logging option for debugging browser interactions
- **PAT-001**: Use Page Object Model pattern for maintainable UI test code
- **PAT-002**: Implement factory pattern for different form types and test scenarios

## 2. Implementation Steps

### Implementation Phase 1

- GOAL-001: Create core console application structure and HTTP client functionality

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create Program.cs with command-line argument parsing and main application flow in `src/Program.cs` | | |
| TASK-002 | Implement HttpClientService class for making HTTP requests and handling responses in `src/Services/HttpClientService.cs` | | |
| TASK-003 | Create Configuration class for storing endpoint URL, credentials, and test settings in `src/Models/Configuration.cs` | | |
| TASK-004 | Implement CommandLineParser class for parsing and validating user input in `src/Utils/CommandLineParser.cs` | | |

### Implementation Phase 2

- GOAL-002: Implement CSRF detection core logic and test scenarios

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | Create CSRFDetector class with main vulnerability detection logic in `src/Services/CSRFDetector.cs` | | |
| TASK-006 | Implement TokenExtractor class for finding CSRF tokens in HTML responses in `src/Utils/TokenExtractor.cs` | | |
| TASK-007 | Create CSRFTestRunner class with methods for testing missing tokens, invalid tokens, and token reuse in `src/Services/CSRFTestRunner.cs` | | |
| TASK-008 | Implement VulnerabilityResult class for storing and formatting test results in `src/Models/VulnerabilityResult.cs` | | |

### Implementation Phase 3

- GOAL-003: Add authentication support and result reporting

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Create AuthenticationService class for handling login and session management in `src/Services/AuthenticationService.cs` | | |
| TASK-009a | Extend AuthenticationService to support Azure Entra (Azure AD) OAuth 2.0/OpenID Connect flows for Blazor Server apps | | |
| TASK-010 | Implement ResultFormatter class for generating console output and optional file reports in `src/Utils/ResultFormatter.cs` | | |
| TASK-011 | Create logging infrastructure using ILogger interface in `src/Services/LoggingService.cs` | | |
| TASK-012 | Implement error handling and graceful failure scenarios in `src/Utils/ErrorHandler.cs` | | |

### Implementation Phase 4

- GOAL-004: Finalize application with configuration and deployment

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | Create appsettings.json for default configuration and test parameters in `appsettings.json` | | |
| TASK-014 | Implement dependency injection container setup in `src/DependencyInjection/ServiceCollectionExtensions.cs` | | |
| TASK-015 | Create comprehensive help system and usage documentation in application | | |
| TASK-016 | Add project file with necessary NuGet packages and build configuration in `CSRFTester.csproj` | | |

## 3. Alternatives

- **ALT-001**: Use browser automation (Playwright/Selenium) - Rejected due to complexity and overhead for simple endpoint testing
- **ALT-002**: Implement as web-based tool with GUI - Rejected due to requirement for simple console application
- **ALT-003**: Use existing security scanning tools like OWASP ZAP - Rejected due to need for custom, lightweight solution
- **ALT-004**: Pure manual testing approach - Rejected due to lack of automation and repeatability

## 4. Dependencies

- **DEP-001**: System.Net.Http namespace for HTTP client functionality and request handling
- **DEP-002**: Microsoft.Extensions.DependencyInjection NuGet package for dependency injection container
- **DEP-003**: Microsoft.Extensions.Logging NuGet package for structured logging capabilities
- **DEP-004**: Microsoft.Extensions.Configuration NuGet package for configuration management
- **DEP-005**: HtmlAgilityPack NuGet package for HTML parsing and CSRF token extraction
- **DEP-006**: Newtonsoft.Json NuGet package for JSON response parsing and configuration
- **DEP-007**: System.CommandLine NuGet package for robust command-line argument parsing
- **DEP-008**: Microsoft.Identity.Client NuGet package for Azure Entra (Azure AD) authentication flows

## 5. Files

- **FILE-001**: `src/Program.cs` - Main console application entry point with command-line parsing and execution flow
- **FILE-002**: `src/Services/HttpClientService.cs` - HTTP client wrapper for making requests to target endpoints
- **FILE-003**: `src/Models/Configuration.cs` - Configuration model for storing endpoint URLs, credentials, and settings
- **FILE-004**: `src/Utils/CommandLineParser.cs` - Command-line argument parsing and validation utility
- **FILE-005**: `src/Services/CSRFDetector.cs` - Core CSRF vulnerability detection logic and test orchestration
- **FILE-006**: `src/Utils/TokenExtractor.cs` - HTML parsing utility for extracting CSRF tokens from responses
- **FILE-007**: `src/Services/CSRFTestRunner.cs` - Test execution engine for running different CSRF attack scenarios
- **FILE-008**: `src/Models/VulnerabilityResult.cs` - Result model for storing test outcomes and vulnerability details
- **FILE-009**: `src/Services/AuthenticationService.cs` - Authentication handling for accessing protected endpoints
- **FILE-010**: `src/Utils/ResultFormatter.cs` - Console output formatting and optional report generation
- **FILE-011**: `src/Services/LoggingService.cs` - Logging infrastructure for audit trails and debugging
- **FILE-012**: `src/Utils/ErrorHandler.cs` - Centralized error handling and graceful failure management
- **FILE-013**: `appsettings.json` - Application configuration file with default settings and test parameters
- **FILE-014**: `src/DependencyInjection/ServiceCollectionExtensions.cs` - Dependency injection container configuration
- **FILE-015**: `CSRFTester.csproj` - .NET project file with NuGet package references and build settings
- **FILE-016**: `README.md` - Usage documentation and examples for the console application

## 6. Testing

- **TEST-001**: Unit tests for TokenExtractor class validating CSRF token extraction from HTML forms and meta tags
- **TEST-002**: Unit tests for CSRFDetector class verifying vulnerability detection logic and test scenarios
- **TEST-003**: Integration tests for HttpClientService class testing HTTP request handling and error scenarios
- **TEST-004**: End-to-end tests using mock HTTP servers to validate complete CSRF detection workflow
- **TEST-005**: Command-line argument parsing tests ensuring proper validation and error handling
- **TEST-006**: Authentication service tests for various login mechanisms and session handling
- **TEST-007**: Result formatting tests verifying console output and optional report generation
- **TEST-008**: Error handling tests ensuring graceful failure in network error and timeout scenarios

## 7. Risks & Assumptions

- **RISK-001**: False positives when legitimate applications use non-standard CSRF protection mechanisms
- **RISK-002**: Network timeouts or connectivity issues may prevent proper testing of remote endpoints
- **RISK-003**: Some applications may have rate limiting that could interfere with testing multiple requests
- **RISK-004**: Complex authentication flows may require manual intervention or additional configuration
- **ASSUMPTION-001**: Target endpoints use standard HTTP methods (POST, PUT, DELETE, PATCH) for state-changing operations
- **ASSUMPTION-002**: CSRF tokens are implemented using common patterns (hidden form fields, meta tags, custom headers)
- **ASSUMPTION-003**: Application uses standard session management (cookies, bearer tokens) for authentication
- **ASSUMPTION-004**: Target endpoint responds with standard HTTP status codes and content types

## 8. Related Specifications / Further Reading

[OWASP Cross-Site Request Forgery Prevention Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Cross-Site_Request_Forgery_Prevention_Cheat_Sheet.html)
[CWE-352: Cross-Site Request Forgery (CSRF)](https://cwe.mitre.org/data/definitions/352.html)
[.NET HttpClient Documentation](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient)
[OWASP Testing Guide - Testing for CSRF](https://owasp.org/www-project-web-security-testing-guide/latest/4-Web_Application_Security_Testing/06-Session_Management_Testing/05-Testing_for_Cross_Site_Request_Forgery)
[System.CommandLine Documentation](https://docs.microsoft.com/en-us/dotnet/standard/commandline/)
[HtmlAgilityPack Documentation](https://html-agility-pack.net/)
