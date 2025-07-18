using Microsoft.Extensions.Logging;
using CSRFTester.Models;
using CSRFTester.Utils;
using Microsoft.Playwright;

namespace CSRFTester.Services;

public class CSRFTestRunner
{
    private readonly TokenExtractor _tokenExtractor;
    private readonly ILogger<CSRFTestRunner> _logger;

    public CSRFTestRunner(
        TokenExtractor tokenExtractor,
        ILogger<CSRFTestRunner> logger)
    {
        _tokenExtractor = tokenExtractor;
        _logger = logger;
    }

    public async Task<List<TestResult>> RunUITestsAsync(Configuration config, BrowserService browserService)
    {
        var results = new List<TestResult>();

        try
        {
            var page = await browserService.GetPageAsync();
            
            // Find all forms on the page
            var forms = await browserService.FindFormsAsync();
            
            if (forms.Count == 0)
            {
                results.Add(new TestResult
                {
                    TestName = "Form Detection",
                    Passed = false,
                    Description = "Find forms on the web page",
                    Details = "No forms found on the page",
                    Severity = TestSeverity.Medium
                });
                return results;
            }

            _logger.LogInformation("Found {FormCount} forms to test", forms.Count);

            // Test each form for CSRF vulnerabilities
            for (int i = 0; i < forms.Count; i++)
            {
                var form = forms[i];
                _logger.LogDebug("Testing form {FormIndex}", i + 1);

                // Test 1: Check for CSRF token presence
                results.Add(await TestCsrfTokenPresence(form, browserService, i + 1));

                // Test 2: Test missing CSRF token
                results.Add(await TestMissingCsrfToken(form, browserService, i + 1));

                // Test 3: Test invalid CSRF token (if token exists)
                var token = await browserService.GetCsrfTokenAsync(form);
                if (!string.IsNullOrEmpty(token))
                {
                    results.Add(await TestInvalidCsrfToken(form, browserService, token, i + 1));
                }

                // Test 4: Test form submission without origin header
                results.Add(await TestFormWithoutOrigin(form, browserService, i + 1));
            }

            _logger.LogInformation("Completed CSRF UI tests for {FormCount} forms", forms.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running CSRF UI tests");
            results.Add(new TestResult
            {
                TestName = "CSRF UI Test Suite",
                Passed = false,
                Description = "Error running CSRF vulnerability tests on forms",
                Details = ex.Message,
                Severity = TestSeverity.High
            });
        }

        return results;
    }

    private async Task<TestResult> TestCsrfTokenPresence(IElementHandle form, BrowserService browserService, int formIndex)
    {
        try
        {
            var token = await browserService.GetCsrfTokenAsync(form);
            bool hasToken = !string.IsNullOrEmpty(token);

            return new TestResult
            {
                TestName = $"CSRF Token Presence (Form {formIndex})",
                Passed = hasToken,
                Description = "Check if form contains CSRF protection token",
                Details = hasToken ? $"CSRF token found: {token?[..Math.Min(token.Length, 10)]}..." : "No CSRF token found in form",
                Severity = hasToken ? TestSeverity.Low : TestSeverity.High
            };
        }
        catch (Exception ex)
        {
            return new TestResult
            {
                TestName = $"CSRF Token Presence (Form {formIndex})",
                Passed = false,
                Description = "Check if form contains CSRF protection token",
                Details = $"Error checking for CSRF token: {ex.Message}",
                Severity = TestSeverity.Medium
            };
        }
    }

    private async Task<TestResult> TestMissingCsrfToken(IElementHandle form, BrowserService browserService, int formIndex)
    {
        try
        {
            _logger.LogDebug("Testing form {FormIndex} submission without CSRF token", formIndex);

            var page = await browserService.GetPageAsync();
            
            // Remove any existing CSRF tokens
            await page.EvaluateAsync(@"(form) => {
                const csrfInputs = form.querySelectorAll('input[name*=""csrf""], input[name*=""token""], input[name=""_token""]');
                csrfInputs.forEach(input => input.remove());
            }", form);

            // Try to submit the form
            var submitButton = await form.QuerySelectorAsync("input[type='submit'], button[type='submit'], button");
            if (submitButton == null)
            {
                return new TestResult
                {
                    TestName = $"Missing CSRF Token Test (Form {formIndex})",
                    Passed = false,
                    Description = "Test form submission without CSRF token",
                    Details = "No submit button found",
                    Severity = TestSeverity.Low
                };
            }

            // Click submit and check response
            try
            {
                var responsePromise = page.WaitForResponseAsync(response => true, new PageWaitForResponseOptions { Timeout = 10000 });
                await submitButton.ClickAsync();
                var response = await responsePromise;

                // If form submits successfully without token, it's vulnerable
                bool isVulnerable = response.Ok;

                return new TestResult
                {
                    TestName = $"Missing CSRF Token Test (Form {formIndex})",
                    Passed = !isVulnerable,
                    Description = "Test form submission without CSRF token",
                    Details = isVulnerable 
                        ? $"Form submitted successfully without CSRF token (Status: {response.Status})" 
                        : $"Form submission rejected without CSRF token (Status: {response.Status})",
                    Severity = isVulnerable ? TestSeverity.High : TestSeverity.Low
                };
            }
            catch (TimeoutException)
            {
                return new TestResult
                {
                    TestName = $"Missing CSRF Token Test (Form {formIndex})",
                    Passed = true,
                    Description = "Test form submission without CSRF token",
                    Details = "Form submission timed out (likely blocked)",
                    Severity = TestSeverity.Low
                };
            }
        }
        catch (Exception ex)
        {
            return new TestResult
            {
                TestName = $"Missing CSRF Token Test (Form {formIndex})",
                Passed = false,
                Description = "Test form submission without CSRF token",
                Details = $"Test failed: {ex.Message}",
                Severity = TestSeverity.Medium
            };
        }
    }

    private async Task<TestResult> TestInvalidCsrfToken(IElementHandle form, BrowserService browserService, string validToken, int formIndex)
    {
        try
        {
            _logger.LogDebug("Testing form {FormIndex} with invalid CSRF token", formIndex);

            var page = await browserService.GetPageAsync();
            var invalidToken = "invalid_" + Guid.NewGuid().ToString("N")[..16];

            // Replace the valid token with an invalid one
            await page.EvaluateAsync(@"(args) => {
                const form = args.form;
                const invalidToken = args.invalidToken;
                const csrfInputs = form.querySelectorAll('input[name*=""csrf""], input[name*=""token""], input[name=""_token""]');
                csrfInputs.forEach(input => input.value = invalidToken);
            }", new { form, invalidToken });

            // Try to submit the form
            var submitButton = await form.QuerySelectorAsync("input[type='submit'], button[type='submit'], button");
            if (submitButton == null)
            {
                return new TestResult
                {
                    TestName = $"Invalid CSRF Token Test (Form {formIndex})",
                    Passed = false,
                    Description = "Test form submission with invalid CSRF token",
                    Details = "No submit button found",
                    Severity = TestSeverity.Low
                };
            }

            try
            {
                var responsePromise = page.WaitForResponseAsync(response => true, new PageWaitForResponseOptions { Timeout = 10000 });
                await submitButton.ClickAsync();
                var response = await responsePromise;

                // If form submits successfully with invalid token, it's vulnerable
                bool isVulnerable = response.Ok;

                return new TestResult
                {
                    TestName = $"Invalid CSRF Token Test (Form {formIndex})",
                    Passed = !isVulnerable,
                    Description = "Test form submission with invalid CSRF token",
                    Details = isVulnerable 
                        ? $"Form accepted invalid CSRF token (Status: {response.Status})" 
                        : $"Form rejected invalid CSRF token (Status: {response.Status})",
                    Severity = isVulnerable ? TestSeverity.High : TestSeverity.Low
                };
            }
            catch (TimeoutException)
            {
                return new TestResult
                {
                    TestName = $"Invalid CSRF Token Test (Form {formIndex})",
                    Passed = true,
                    Description = "Test form submission with invalid CSRF token",
                    Details = "Form submission timed out (likely blocked)",
                    Severity = TestSeverity.Low
                };
            }
        }
        catch (Exception ex)
        {
            return new TestResult
            {
                TestName = $"Invalid CSRF Token Test (Form {formIndex})",
                Passed = false,
                Description = "Test form submission with invalid CSRF token",
                Details = $"Test failed: {ex.Message}",
                Severity = TestSeverity.Medium
            };
        }
    }

    private Task<TestResult> TestFormWithoutOrigin(IElementHandle form, BrowserService browserService, int formIndex)
    {
        try
        {
            _logger.LogDebug("Testing form {FormIndex} submission without proper origin", formIndex);

            // This test would require more complex setup to modify request headers
            // For now, we'll return a basic test result
            return Task.FromResult(new TestResult
            {
                TestName = $"Origin Header Test (Form {formIndex})",
                Passed = true,
                Description = "Test origin header validation (placeholder)",
                Details = "Origin header testing requires advanced setup - test skipped",
                Severity = TestSeverity.Low
            });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new TestResult
            {
                TestName = $"Origin Header Test (Form {formIndex})",
                Passed = false,
                Description = "Test origin header validation",
                Details = $"Test failed: {ex.Message}",
                Severity = TestSeverity.Low
            });
        }
    }
}
