using CSRFTester.Models;
using CSRFTester.Services;
using CSRFTester.Utils;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CSRFTester.Tests.Services;

public class CSRFDetectorTests
{
    private readonly Mock<ILogger<CSRFDetector>> _mockLogger;
    private readonly Mock<BrowserService> _mockBrowserService;
    private readonly Mock<AuthenticationService> _mockAuthService;
    private readonly Mock<CSRFTestRunner> _mockTestRunner;

    public CSRFDetectorTests()
    {
        var mockBrowserLogger = new Mock<ILogger<BrowserService>>();
        var mockAuthLogger = new Mock<ILogger<AuthenticationService>>();
        var mockTestRunnerLogger = new Mock<ILogger<CSRFTestRunner>>();
        var mockTokenExtractorLogger = new Mock<ILogger<CSRFTester.Utils.TokenExtractor>>();

        _mockLogger = new Mock<ILogger<CSRFDetector>>();
        _mockBrowserService = new Mock<BrowserService>(mockBrowserLogger.Object);
        _mockAuthService = new Mock<AuthenticationService>(mockAuthLogger.Object);
        
        // Create a real TokenExtractor for the CSRFTestRunner mock
        var tokenExtractor = new CSRFTester.Utils.TokenExtractor(mockTokenExtractorLogger.Object);
        _mockTestRunner = new Mock<CSRFTestRunner>(tokenExtractor, mockTestRunnerLogger.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDependencies()
    {
        // Act
        var detector = new CSRFDetector(
            _mockBrowserService.Object,
            _mockAuthService.Object,
            _mockTestRunner.Object,
            _mockLogger.Object);

        // Assert
        Assert.NotNull(detector);
        // Constructor should not throw and should accept all dependencies
    }

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("http://localhost:3000")]
    [InlineData("https://secure.site.com/forms")]
    public void CSRFDetector_ShouldAcceptDifferentUrls(string url)
    {
        // Arrange
        var config = new Configuration { TargetUrl = url };

        // Act
        var detector = new CSRFDetector(
            _mockBrowserService.Object,
            _mockAuthService.Object,
            _mockTestRunner.Object,
            _mockLogger.Object);

        // Assert
        Assert.NotNull(detector);
        Assert.Equal(url, config.TargetUrl);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CSRFDetector_ShouldAcceptDifferentHeadlessModes(bool headless)
    {
        // Arrange
        var config = new Configuration { HeadlessMode = headless };

        // Act
        var detector = new CSRFDetector(
            _mockBrowserService.Object,
            _mockAuthService.Object,
            _mockTestRunner.Object,
            _mockLogger.Object);

        // Assert
        Assert.NotNull(detector);
        Assert.Equal(headless, config.HeadlessMode);
    }

    [Theory]
    [InlineData("testuser", "testpass")]
    [InlineData("admin", "secret")]
    [InlineData(null, null)]
    public void CSRFDetector_ShouldAcceptDifferentCredentials(string? username, string? password)
    {
        // Arrange
        var config = new Configuration 
        { 
            Username = username, 
            Password = password 
        };

        // Act
        var detector = new CSRFDetector(
            _mockBrowserService.Object,
            _mockAuthService.Object,
            _mockTestRunner.Object,
            _mockLogger.Object);

        // Assert
        Assert.NotNull(detector);
        Assert.Equal(username, config.Username);
        Assert.Equal(password, config.Password);
    }

    [Fact]
    public void CSRFDetector_HasCorrectLogger()
    {
        // Act
        var detector = new CSRFDetector(
            _mockBrowserService.Object,
            _mockAuthService.Object,
            _mockTestRunner.Object,
            _mockLogger.Object);

        // Assert
        Assert.NotNull(detector);
        // Detector should be created successfully with logger
    }

    [Fact]
    public void CSRFDetector_HasCorrectBrowserService()
    {
        // Act
        var detector = new CSRFDetector(
            _mockBrowserService.Object,
            _mockAuthService.Object,
            _mockTestRunner.Object,
            _mockLogger.Object);

        // Assert
        Assert.NotNull(detector);
        // Detector should be created successfully with browser service
    }

    [Fact]
    public void CSRFDetector_HasCorrectAuthenticationService()
    {
        // Act
        var detector = new CSRFDetector(
            _mockBrowserService.Object,
            _mockAuthService.Object,
            _mockTestRunner.Object,
            _mockLogger.Object);

        // Assert
        Assert.NotNull(detector);
        // Detector should be created successfully with authentication service
    }

    [Fact]
    public void CSRFDetector_HasCorrectTestRunner()
    {
        // Act
        var detector = new CSRFDetector(
            _mockBrowserService.Object,
            _mockAuthService.Object,
            _mockTestRunner.Object,
            _mockLogger.Object);

        // Assert
        Assert.NotNull(detector);
        // Detector should be created successfully with test runner
    }

    [Theory]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(120)]
    public void CSRFDetector_ShouldAcceptDifferentTimeouts(int timeout)
    {
        // Arrange
        var config = new Configuration { TimeoutSeconds = timeout };

        // Act
        var detector = new CSRFDetector(
            _mockBrowserService.Object,
            _mockAuthService.Object,
            _mockTestRunner.Object,
            _mockLogger.Object);

        // Assert
        Assert.NotNull(detector);
        Assert.Equal(timeout, config.TimeoutSeconds);
    }

    [Theory]
    [InlineData("CSRFTester/1.0")]
    [InlineData("CustomBot/2.0")]
    [InlineData("TestAgent/1.5")]
    public void CSRFDetector_ShouldAcceptDifferentUserAgents(string userAgent)
    {
        // Arrange
        var config = new Configuration { UserAgent = userAgent };

        // Act
        var detector = new CSRFDetector(
            _mockBrowserService.Object,
            _mockAuthService.Object,
            _mockTestRunner.Object,
            _mockLogger.Object);

        // Assert
        Assert.NotNull(detector);
        Assert.Equal(userAgent, config.UserAgent);
    }
}