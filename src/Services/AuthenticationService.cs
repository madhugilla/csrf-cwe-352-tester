using Microsoft.Extensions.Logging;
using CSRFTester.Models;

namespace CSRFTester.Services;

public class AuthenticationService
{
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(ILogger<AuthenticationService> logger)
    {
        _logger = logger;
    }

    public async Task<TestResult> AuthenticateAsync(Configuration config, BrowserService browserService)
    {
        try
        {
            _logger.LogInformation("Attempting authentication for user: {Username}", config.Username);

            var page = await browserService.GetPageAsync();

            // Look for login form on the current page
            var loginForm = await page.QuerySelectorAsync("form[action*='login'], form[id*='login'], form[class*='login'], form");
            
            if (loginForm == null)
            {
                return new TestResult
                {
                    TestName = "Authentication",
                    Passed = false,
                    Description = "Authenticate with provided credentials",
                    Details = "No login form found on the page",
                    Severity = TestSeverity.Medium
                };
            }

            // Try to find username and password fields
            var usernameField = await loginForm.QuerySelectorAsync("input[type='text'], input[type='email'], input[name*='user'], input[name*='email'], input[id*='user'], input[id*='email']");
            var passwordField = await loginForm.QuerySelectorAsync("input[type='password']");

            if (usernameField == null || passwordField == null)
            {
                return new TestResult
                {
                    TestName = "Authentication",
                    Passed = false,
                    Description = "Authenticate with provided credentials",
                    Details = "Could not find username or password fields",
                    Severity = TestSeverity.Medium
                };
            }

            // Fill in credentials
            await usernameField.FillAsync(config.Username ?? "");
            await passwordField.FillAsync(config.Password ?? "");

            // Submit the form
            var submitButton = await loginForm.QuerySelectorAsync("input[type='submit'], button[type='submit'], button");
            if (submitButton != null)
            {
                await submitButton.ClickAsync();
                await page.WaitForLoadStateAsync();
            }

            // Check if login was successful (basic check - no error messages visible)
            var errorElements = await page.QuerySelectorAllAsync(".error, .alert-danger, [class*='error'], [class*='invalid']");
            bool authSuccessful = errorElements.Count == 0;

            return new TestResult
            {
                TestName = "Authentication",
                Passed = authSuccessful,
                Description = "Authenticate with provided credentials",
                Details = authSuccessful ? "Authentication appears successful" : "Authentication may have failed (error elements detected)",
                Severity = authSuccessful ? TestSeverity.Low : TestSeverity.Medium
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication failed");
            return new TestResult
            {
                TestName = "Authentication",
                Passed = false,
                Description = "Authenticate with provided credentials",
                Details = $"Authentication error: {ex.Message}",
                Severity = TestSeverity.Medium
            };
        }
    }
}
