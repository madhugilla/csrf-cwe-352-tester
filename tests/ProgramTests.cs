using CSRFTester;
using CSRFTester.Models;
using CSRFTester.Services;
using CSRFTester.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CSRFTester.Tests;

public class ProgramTests
{
    [Fact]
    public void Program_ServiceConfiguration_ShouldRegisterAllServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - This simulates the ConfigureServices method from Program.cs
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        services.AddHttpClient<HttpClientService>();
        services.AddTransient<BrowserService>();
        services.AddTransient<CSRFDetector>();
        services.AddTransient<CSRFTestRunner>();
        services.AddTransient<AuthenticationService>();
        services.AddTransient<TokenExtractor>();
        services.AddTransient<ResultFormatter>();
        services.AddTransient<ErrorHandler>();

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<ILogger<CSRFDetector>>());
        Assert.NotNull(serviceProvider.GetService<HttpClientService>());
        Assert.NotNull(serviceProvider.GetService<BrowserService>());
        Assert.NotNull(serviceProvider.GetService<CSRFDetector>());
        Assert.NotNull(serviceProvider.GetService<CSRFTestRunner>());
        Assert.NotNull(serviceProvider.GetService<AuthenticationService>());
        Assert.NotNull(serviceProvider.GetService<TokenExtractor>());
        Assert.NotNull(serviceProvider.GetService<ResultFormatter>());
        Assert.NotNull(serviceProvider.GetService<ErrorHandler>());
    }

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("http://localhost:3000")]
    [InlineData("https://test.site.com/form")]
    public void Configuration_WithValidUrls_ShouldCreateSuccessfully(string url)
    {
        // Arrange & Act
        var config = new Configuration
        {
            TargetUrl = url,
            HeadlessMode = true,
            VerboseLogging = false
        };

        // Assert
        Assert.Equal(url, config.TargetUrl);
        Assert.True(config.HeadlessMode);
        Assert.False(config.VerboseLogging);
    }

    [Theory]
    [InlineData("testuser", "testpass", true)]
    [InlineData("admin", "secret123", false)]
    [InlineData(null, null, true)]
    public void Configuration_WithCredentials_ShouldCreateSuccessfully(string? username, string? password, bool headless)
    {
        // Arrange & Act
        var config = new Configuration
        {
            TargetUrl = "https://example.com/login",
            Username = username,
            Password = password,
            HeadlessMode = headless
        };

        // Assert
        Assert.Equal("https://example.com/login", config.TargetUrl);
        Assert.Equal(username, config.Username);
        Assert.Equal(password, config.Password);
        Assert.Equal(headless, config.HeadlessMode);
    }

    [Fact]
    public void VulnerabilityResult_Construction_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var result = new VulnerabilityResult
        {
            TargetUrl = "https://example.com/test",
            TestDate = DateTime.UtcNow,
            IsVulnerable = false,
            Summary = "No vulnerabilities found"
        };

        result.TestResults.Add(new TestResult
        {
            TestName = "Test 1",
            Passed = true,
            Description = "Test description",
            Severity = TestSeverity.Low
        });

        result.Recommendations.Add("Use CSRF tokens");

        // Assert
        Assert.Equal("https://example.com/test", result.TargetUrl);
        Assert.False(result.IsVulnerable);
        Assert.Equal("No vulnerabilities found", result.Summary);
        Assert.Single(result.TestResults);
        Assert.Single(result.Recommendations);
        Assert.Equal("Test 1", result.TestResults[0].TestName);
        Assert.Equal("Use CSRF tokens", result.Recommendations[0]);
    }

    [Theory]
    [InlineData(TestSeverity.Low)]
    [InlineData(TestSeverity.Medium)]
    [InlineData(TestSeverity.High)]
    [InlineData(TestSeverity.Critical)]
    public void TestResult_WithDifferentSeverities_ShouldCreateCorrectly(TestSeverity severity)
    {
        // Arrange & Act
        var testResult = new TestResult
        {
            TestName = "Test",
            Passed = false,
            Description = "Test description",
            Details = "Test details",
            Severity = severity
        };

        // Assert
        Assert.Equal("Test", testResult.TestName);
        Assert.False(testResult.Passed);
        Assert.Equal("Test description", testResult.Description);
        Assert.Equal("Test details", testResult.Details);
        Assert.Equal(severity, testResult.Severity);
    }

    [Fact]
    public void ServiceCollection_Integration_ShouldResolveAllDependencies()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpClient<HttpClientService>();
        services.AddTransient<BrowserService>();
        services.AddTransient<TokenExtractor>();
        services.AddTransient<CSRFTestRunner>();
        services.AddTransient<AuthenticationService>();
        services.AddTransient<CSRFDetector>();
        services.AddTransient<ResultFormatter>();
        services.AddTransient<ErrorHandler>();

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert - Verify all services can be resolved
        var detector = serviceProvider.GetRequiredService<CSRFDetector>();
        var formatter = serviceProvider.GetRequiredService<ResultFormatter>();
        var errorHandler = serviceProvider.GetRequiredService<ErrorHandler>();
        var tokenExtractor = serviceProvider.GetRequiredService<TokenExtractor>();

        Assert.NotNull(detector);
        Assert.NotNull(formatter);
        Assert.NotNull(errorHandler);
        Assert.NotNull(tokenExtractor);
    }
}