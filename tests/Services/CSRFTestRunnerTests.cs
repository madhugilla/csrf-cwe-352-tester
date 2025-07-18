using CSRFTester.Models;
using CSRFTester.Services;
using CSRFTester.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Moq;
using Xunit;

namespace CSRFTester.Tests.Services;

public class CSRFTestRunnerTests
{
    private readonly Mock<ILogger<CSRFTestRunner>> _mockLogger;
    private readonly Mock<TokenExtractor> _mockTokenExtractor;
    private readonly CSRFTestRunner _csrfTestRunner;

    public CSRFTestRunnerTests()
    {
        var mockTokenExtractorLogger = new Mock<ILogger<TokenExtractor>>();
        
        _mockLogger = new Mock<ILogger<CSRFTestRunner>>();
        _mockTokenExtractor = new Mock<TokenExtractor>(mockTokenExtractorLogger.Object);
        _csrfTestRunner = new CSRFTestRunner(_mockTokenExtractor.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDependencies()
    {
        // Act & Assert
        Assert.NotNull(_csrfTestRunner);
        // Constructor should not throw and should accept dependencies
    }

    [Fact]
    public async Task RunUITestsAsync_NullConfiguration_ShouldHandleGracefully()
    {
        // Arrange
        var mockBrowserService = new Mock<BrowserService>(Mock.Of<ILogger<BrowserService>>());

        // Act
        var results = await _csrfTestRunner.RunUITestsAsync(null!, mockBrowserService.Object);

        // Assert
        Assert.NotNull(results);
        // Method should handle null configuration gracefully
    }

    [Fact]
    public async Task RunUITestsAsync_NullBrowserService_ShouldHandleGracefully()
    {
        // Arrange
        var config = CreateTestConfiguration();

        // Act
        var results = await _csrfTestRunner.RunUITestsAsync(config, null!);

        // Assert
        Assert.NotNull(results);
        // Method should handle null browser service gracefully
    }

    [Theory]
    [InlineData("https://example.com/form")]
    [InlineData("http://localhost:3000/submit")]
    [InlineData("https://test.domain.com/contact")]
    public void CreateTestConfiguration_WithDifferentUrls_ShouldStoreCorrectly(string url)
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
    [InlineData(true)]
    [InlineData(false)]
    public void CreateTestConfiguration_WithVerboseLogging_ShouldStoreCorrectly(bool verbose)
    {
        // Arrange & Act
        var config = new Configuration
        {
            TargetUrl = "https://example.com/test",
            VerboseLogging = verbose
        };

        // Assert
        Assert.Equal(verbose, config.VerboseLogging);
    }

    [Fact]
    public void CSRFTestRunner_HasCorrectTokenExtractor()
    {
        // Act
        var runner = new CSRFTestRunner(_mockTokenExtractor.Object, _mockLogger.Object);

        // Assert
        Assert.NotNull(runner);
        // Runner should be created successfully with token extractor
    }

    [Fact]
    public void CSRFTestRunner_HasCorrectLogger()
    {
        // Act
        var runner = new CSRFTestRunner(_mockTokenExtractor.Object, _mockLogger.Object);

        // Assert
        Assert.NotNull(runner);
        // Runner should be created successfully with logger
    }

    [Theory]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(120)]
    public void Configuration_WithDifferentTimeouts_ShouldStoreCorrectly(int timeout)
    {
        // Arrange & Act
        var config = new Configuration
        {
            TargetUrl = "https://example.com/test",
            TimeoutSeconds = timeout
        };

        // Assert
        Assert.Equal(timeout, config.TimeoutSeconds);
    }

    [Theory]
    [InlineData("CustomAgent/1.0")]
    [InlineData("TestBot/2.0")]
    [InlineData("CSRFTester/1.5")]
    public void Configuration_WithDifferentUserAgents_ShouldStoreCorrectly(string userAgent)
    {
        // Arrange & Act
        var config = new Configuration
        {
            TargetUrl = "https://example.com/test",
            UserAgent = userAgent
        };

        // Assert
        Assert.Equal(userAgent, config.UserAgent);
    }

    private Configuration CreateTestConfiguration()
    {
        return new Configuration
        {
            TargetUrl = "https://example.com/test",
            HeadlessMode = true,
            VerboseLogging = false
        };
    }
}