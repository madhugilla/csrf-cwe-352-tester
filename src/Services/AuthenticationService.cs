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
            if (config.UseEntraAuth)
            {
                return await AuthenticateWithEntraAsync(config, browserService);
            }
            else
            {
                return await AuthenticateWithFormAsync(config, browserService);
            }
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

    private async Task<TestResult> AuthenticateWithEntraAsync(Configuration config, BrowserService browserService)
    {
        _logger.LogInformation("Attempting Azure Entra authentication for tenant: {TenantId}", config.EntraTenantId);

        var page = await browserService.GetPageAsync();

        // Navigate to the target URL first to trigger Azure AD authentication
        await page.GotoAsync(config.TargetUrl);
        await page.WaitForLoadStateAsync();

        // Check if we're redirected to Azure AD login page
        var currentUrl = page.Url;
        if (currentUrl.Contains("login.microsoftonline.com") || currentUrl.Contains("microsoft.com/oauth"))
        {
            _logger.LogInformation("Detected Azure AD redirect, handling authentication flow");

            // If username and password are provided, attempt automatic login
            if (!string.IsNullOrEmpty(config.Username) && !string.IsNullOrEmpty(config.Password))
            {
                await HandleAutomaticEntraLogin(page, config);
            }
            else
            {
                _logger.LogInformation("No credentials provided for Azure Entra. Waiting for manual authentication...");
                _logger.LogInformation("Please complete the authentication in the browser and press Enter to continue...");
                
                if (!config.HeadlessMode)
                {
                    Console.WriteLine("Please complete the Azure Entra authentication in the browser window and press Enter to continue...");
                    Console.ReadLine();
                }
                else
                {
                    _logger.LogWarning("Running in headless mode but no credentials provided for Azure Entra authentication");
                    return new TestResult
                    {
                        TestName = "Azure Entra Authentication",
                        Passed = false,
                        Description = "Authenticate with Azure Entra",
                        Details = "No credentials provided and running in headless mode",
                        Severity = TestSeverity.Medium
                    };
                }
            }

            // Wait for redirect back to the application
            try
            {
                await page.WaitForURLAsync(url => !url.Contains("login.microsoftonline.com") && !url.Contains("microsoft.com/oauth"), 
                    new() { Timeout = 30000 });
            }
            catch (TimeoutException)
            {
                return new TestResult
                {
                    TestName = "Azure Entra Authentication",
                    Passed = false,
                    Description = "Authenticate with Azure Entra",
                    Details = "Timeout waiting for authentication redirect",
                    Severity = TestSeverity.Medium
                };
            }

            // Check if authentication was successful
            var finalUrl = page.Url;
            bool authSuccessful = !finalUrl.Contains("login.microsoftonline.com") && 
                                 !finalUrl.Contains("microsoft.com/oauth") &&
                                 !finalUrl.Contains("error");

            return new TestResult
            {
                TestName = "Azure Entra Authentication",
                Passed = authSuccessful,
                Description = "Authenticate with Azure Entra",
                Details = authSuccessful ? 
                    $"Azure Entra authentication successful, redirected to: {finalUrl}" : 
                    "Azure Entra authentication may have failed",
                Severity = authSuccessful ? TestSeverity.Low : TestSeverity.Medium
            };
        }
        else
        {
            // Not redirected to Azure AD, might be already authenticated or different auth method
            return new TestResult
            {
                TestName = "Azure Entra Authentication",
                Passed = true,
                Description = "Authenticate with Azure Entra",
                Details = "No Azure AD redirect detected, may already be authenticated or using different authentication",
                Severity = TestSeverity.Low
            };
        }
    }

    private async Task HandleAutomaticEntraLogin(Microsoft.Playwright.IPage page, Configuration config)
    {
        try
        {
            // Wait for and fill username field
            var usernameField = await page.WaitForSelectorAsync("input[type='email'], input[name='loginfmt'], input[placeholder*='email']", 
                new() { Timeout = 10000 });
            if (usernameField != null)
            {
                await usernameField.FillAsync(config.Username!);
                
                // Click Next button
                var nextButton = await page.QuerySelectorAsync("input[type='submit'], button[type='submit'], input[value='Next']");
                if (nextButton != null)
                {
                    await nextButton.ClickAsync();
                    await page.WaitForLoadStateAsync();
                }
            }

            // Wait for and fill password field  
            var passwordField = await page.WaitForSelectorAsync("input[type='password'], input[name='passwd']", 
                new() { Timeout = 10000 });
            if (passwordField != null)
            {
                await passwordField.FillAsync(config.Password!);
                
                // Click Sign In button
                var signInButton = await page.QuerySelectorAsync("input[type='submit'], button[type='submit'], input[value*='Sign in']");
                if (signInButton != null)
                {
                    await signInButton.ClickAsync();
                    await page.WaitForLoadStateAsync();
                }
            }

            // Handle "Stay signed in?" prompt if it appears
            try
            {
                var staySignedInButton = await page.WaitForSelectorAsync("input[value='Yes'], button:has-text('Yes')", 
                    new() { Timeout = 5000 });
                if (staySignedInButton != null)
                {
                    await staySignedInButton.ClickAsync();
                    await page.WaitForLoadStateAsync();
                }
            }
            catch (TimeoutException)
            {
                // "Stay signed in" prompt may not appear, continue
            }

            _logger.LogInformation("Automatic Azure Entra login attempted");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during automatic Azure Entra login, may require manual intervention");
        }
    }

    private async Task<TestResult> AuthenticateWithFormAsync(Configuration config, BrowserService browserService)
    {
        _logger.LogInformation("Attempting form-based authentication for user: {Username}", config.Username);

        var page = await browserService.GetPageAsync();

        // Look for login form on the current page
        var loginForm = await page.QuerySelectorAsync("form[action*='login'], form[id*='login'], form[class*='login'], form");
        
        if (loginForm == null)
        {
            return new TestResult
            {
                TestName = "Form Authentication",
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
                TestName = "Form Authentication",
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
            TestName = "Form Authentication",
            Passed = authSuccessful,
            Description = "Authenticate with provided credentials",
            Details = authSuccessful ? "Authentication appears successful" : "Authentication may have failed (error elements detected)",
            Severity = authSuccessful ? TestSeverity.Low : TestSeverity.Medium
        };
    }
}
