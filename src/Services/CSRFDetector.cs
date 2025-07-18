using Microsoft.Extensions.Logging;
using CSRFTester.Models;

namespace CSRFTester.Services;

public class CSRFDetector
{
    private readonly BrowserService _browserService;
    private readonly AuthenticationService _authService;
    private readonly CSRFTestRunner _testRunner;
    private readonly ILogger<CSRFDetector> _logger;

    public CSRFDetector(
        BrowserService browserService,
        AuthenticationService authService,
        CSRFTestRunner testRunner,
        ILogger<CSRFDetector> logger)
    {
        _browserService = browserService;
        _authService = authService;
        _testRunner = testRunner;
        _logger = logger;
    }

    public async Task<VulnerabilityResult> TestWebPageAsync(Configuration config)
    {
        var result = new VulnerabilityResult
        {
            TargetUrl = config.TargetUrl,
            TestDate = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("Starting CSRF UI vulnerability assessment for: {Url}", config.TargetUrl);

            // Step 1: Initialize browser
            await _browserService.InitializeAsync("msedge", config.HeadlessMode);

            // Step 2: Test page accessibility
            var accessibilityTest = await TestPageAccessibility(config.TargetUrl);
            result.TestResults.Add(accessibilityTest);

            if (!accessibilityTest.Passed)
            {
                result.Summary = "Web page is not accessible";
                return result;
            }

            // Step 3: Authenticate if credentials provided
            if (!string.IsNullOrEmpty(config.Username) && !string.IsNullOrEmpty(config.Password))
            {
                var authTest = await _authService.AuthenticateAsync(config, _browserService);
                result.TestResults.Add(authTest);
            }

            // Step 4: Find and test forms for CSRF vulnerabilities
            var csrfTests = await _testRunner.RunUITestsAsync(config, _browserService);
            result.TestResults.AddRange(csrfTests);

            // Step 5: Analyze results and determine overall vulnerability
            AnalyzeResults(result);

            _logger.LogInformation("CSRF UI vulnerability assessment completed. Vulnerable: {IsVulnerable}", result.IsVulnerable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during CSRF UI vulnerability assessment");
            result.TestResults.Add(new TestResult
            {
                TestName = "Error",
                Passed = false,
                Description = "An error occurred during UI testing",
                Details = ex.Message,
                Severity = TestSeverity.High
            });
        }
        finally
        {
            // Cleanup browser resources
            await _browserService.DisposeAsync();
        }

        return result;
    }

    private async Task<TestResult> TestPageAccessibility(string url)
    {
        try
        {
            await _browserService.NavigateToAsync(url);
            var title = await _browserService.GetPageTitleAsync();
            
            return new TestResult
            {
                TestName = "Page Accessibility",
                Passed = true,
                Description = "Check if the target web page is accessible",
                Details = $"Successfully loaded page: {title}",
                Severity = TestSeverity.Low
            };
        }
        catch (Exception ex)
        {
            return new TestResult
            {
                TestName = "Page Accessibility",
                Passed = false,
                Description = "Check if the target web page is accessible",
                Details = $"Failed to load page: {ex.Message}",
                Severity = TestSeverity.High
            };
        }
    }

    private void AnalyzeResults(VulnerabilityResult result)
    {
        var vulnerabilityTests = result.TestResults.Where(t => 
            t.TestName.Contains("CSRF", StringComparison.OrdinalIgnoreCase) && 
            !t.Passed).ToList();

        result.IsVulnerable = vulnerabilityTests.Any();

        if (result.IsVulnerable)
        {
            result.Summary = $"ENDPOINT IS VULNERABLE TO CSRF ATTACKS (CWE-352) - {vulnerabilityTests.Count} vulnerabilities found";
            result.Recommendations.AddRange(new[]
            {
                "Implement CSRF token validation for state-changing operations",
                "Use SameSite cookie attributes (SameSite=Strict or SameSite=Lax)",
                "Validate Origin and Referer headers",
                "Consider using double-submit cookie pattern",
                "Implement proper session management"
            });
        }
        else
        {
            result.Summary = "Endpoint appears to have adequate CSRF protection";
            result.Recommendations.Add("Continue monitoring and regular security assessments");
        }
    }
}
