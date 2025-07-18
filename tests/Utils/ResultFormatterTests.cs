using CSRFTester.Models;
using CSRFTester.Utils;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Text;

namespace CSRFTester.Tests.Utils;

public class ResultFormatterTests
{
    private readonly Mock<ILogger<ResultFormatter>> _mockLogger;
    private readonly ResultFormatter _resultFormatter;

    public ResultFormatterTests()
    {
        _mockLogger = new Mock<ILogger<ResultFormatter>>();
        _resultFormatter = new ResultFormatter(_mockLogger.Object);
    }

    [Fact]
    public async Task DisplayResultsAsync_ValidResult_ShouldDisplayToConsole()
    {
        // Arrange
        var result = CreateSampleVulnerabilityResult();
        var config = CreateSampleConfiguration();

        // Capture console output
        var originalOut = Console.Out;
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            // Act
            await _resultFormatter.DisplayResultsAsync(result, config);

            // Assert
            var output = stringWriter.ToString();
            Assert.Contains("CSRF VULNERABILITY TEST RESULTS", output);
            Assert.Contains(result.TargetUrl, output);
            Assert.Contains("TEST RESULTS:", output);
            Assert.Contains("Sample Test", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public async Task DisplayResultsAsync_VulnerableResult_ShouldDisplayVulnerableStatus()
    {
        // Arrange
        var result = CreateSampleVulnerabilityResult();
        result.IsVulnerable = true;
        result.Summary = "CSRF vulnerability detected";
        var config = CreateSampleConfiguration();

        var originalOut = Console.Out;
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            // Act
            await _resultFormatter.DisplayResultsAsync(result, config);

            // Assert
            var output = stringWriter.ToString();
            Assert.Contains("🚨 VULNERABLE", output);
            Assert.Contains("CSRF vulnerability detected", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public async Task DisplayResultsAsync_SecureResult_ShouldDisplaySecureStatus()
    {
        // Arrange
        var result = CreateSampleVulnerabilityResult();
        result.IsVulnerable = false;
        result.Summary = "No vulnerabilities found";
        var config = CreateSampleConfiguration();

        var originalOut = Console.Out;
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            // Act
            await _resultFormatter.DisplayResultsAsync(result, config);

            // Assert
            var output = stringWriter.ToString();
            Assert.Contains("✅ SECURE", output);
            Assert.Contains("No vulnerabilities found", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public async Task DisplayResultsAsync_ResultWithRecommendations_ShouldDisplayRecommendations()
    {
        // Arrange
        var result = CreateSampleVulnerabilityResult();
        result.Recommendations.AddRange(new[]
        {
            "Implement CSRF tokens",
            "Use SameSite cookies",
            "Validate Origin headers"
        });
        var config = CreateSampleConfiguration();

        var originalOut = Console.Out;
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            // Act
            await _resultFormatter.DisplayResultsAsync(result, config);

            // Assert
            var output = stringWriter.ToString();
            Assert.Contains("RECOMMENDATIONS:", output);
            Assert.Contains("• Implement CSRF tokens", output);
            Assert.Contains("• Use SameSite cookies", output);
            Assert.Contains("• Validate Origin headers", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public async Task DisplayResultsAsync_VerboseLogging_ShouldDisplayTestDetails()
    {
        // Arrange
        var result = CreateSampleVulnerabilityResult();
        result.TestResults.Add(new TestResult
        {
            TestName = "Detailed Test",
            Passed = true,
            Description = "This is a detailed test",
            Details = "Additional details about the test",
            Severity = TestSeverity.Medium
        });
        
        var config = CreateSampleConfiguration();
        config.VerboseLogging = true;

        var originalOut = Console.Out;
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            // Act
            await _resultFormatter.DisplayResultsAsync(result, config);

            // Assert
            var output = stringWriter.ToString();
            Assert.Contains("Description: This is a detailed test", output);
            Assert.Contains("Details: Additional details about the test", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public async Task DisplayResultsAsync_WithOutputFile_ShouldSaveToFile()
    {
        // Arrange
        var result = CreateSampleVulnerabilityResult();
        var config = CreateSampleConfiguration();
        var tempFile = Path.GetTempFileName();
        config.OutputFilePath = tempFile;

        try
        {
            // Act
            await _resultFormatter.DisplayResultsAsync(result, config);

            // Assert
            Assert.True(File.Exists(tempFile));
            var fileContent = await File.ReadAllTextAsync(tempFile);
            Assert.Contains(result.TargetUrl, fileContent);
            Assert.Contains("Sample Test", fileContent);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task DisplayResultsAsync_FailedTest_ShouldDisplayFailureDetails()
    {
        // Arrange
        var result = CreateSampleVulnerabilityResult();
        result.TestResults.Clear();
        result.TestResults.Add(new TestResult
        {
            TestName = "Failed Test",
            Passed = false,
            Description = "This test failed",
            Details = "Failure reason details",
            Severity = TestSeverity.High
        });
        
        var config = CreateSampleConfiguration();
        config.VerboseLogging = false; // Even without verbose, failed tests should show details

        var originalOut = Console.Out;
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            // Act
            await _resultFormatter.DisplayResultsAsync(result, config);

            // Assert
            var output = stringWriter.ToString();
            Assert.Contains("✗ FAIL", output);
            Assert.Contains("Description: This test failed", output);
            Assert.Contains("Details: Failure reason details", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public async Task DisplayResultsAsync_InvalidOutputPath_ShouldHandleGracefully()
    {
        // Arrange
        var result = CreateSampleVulnerabilityResult();
        var config = CreateSampleConfiguration();
        config.OutputFilePath = "/invalid/path/that/does/not/exist/output.json";

        // Act & Assert - Should not throw exception
        await _resultFormatter.DisplayResultsAsync(result, config);
        
        // Verify error was logged (though we can't easily test console output here)
        // The method should handle the exception gracefully
    }

    private VulnerabilityResult CreateSampleVulnerabilityResult()
    {
        return new VulnerabilityResult
        {
            TargetUrl = "https://example.com/test",
            TestDate = new DateTime(2023, 12, 25, 10, 30, 45, DateTimeKind.Utc),
            IsVulnerable = false,
            Summary = "Test completed successfully",
            TestResults = new List<TestResult>
            {
                new TestResult
                {
                    TestName = "Sample Test",
                    Passed = true,
                    Description = "Sample test description",
                    Severity = TestSeverity.Low
                }
            },
            Recommendations = new List<string>()
        };
    }

    private Configuration CreateSampleConfiguration()
    {
        return new Configuration
        {
            TargetUrl = "https://example.com/test",
            VerboseLogging = false,
            HeadlessMode = true
        };
    }
}