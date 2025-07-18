using CSRFTester.Models;
using Xunit;

namespace CSRFTester.Tests.Models;

public class ConfigurationTests
{
    [Fact]
    public void Configuration_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var config = new Configuration();

        // Assert
        Assert.Equal(string.Empty, config.TargetUrl);
        Assert.Null(config.Username);
        Assert.Null(config.Password);
        Assert.True(config.HeadlessMode);
        Assert.False(config.VerboseLogging);
        Assert.Null(config.OutputFilePath);
        Assert.Equal(60, config.TimeoutSeconds);
        Assert.Equal("CSRFTester/1.0", config.UserAgent);
        Assert.Equal("msedge", config.BrowserType);
    }

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("http://localhost:8080")]
    [InlineData("https://test.domain.com/path")]
    public void Configuration_SetTargetUrl_ShouldStoreCorrectly(string url)
    {
        // Arrange
        var config = new Configuration();

        // Act
        config.TargetUrl = url;

        // Assert
        Assert.Equal(url, config.TargetUrl);
    }

    [Theory]
    [InlineData("testuser", "testpass")]
    [InlineData("admin", "secret123")]
    [InlineData(null, null)]
    public void Configuration_SetCredentials_ShouldStoreCorrectly(string? username, string? password)
    {
        // Arrange
        var config = new Configuration();

        // Act
        config.Username = username;
        config.Password = password;

        // Assert
        Assert.Equal(username, config.Username);
        Assert.Equal(password, config.Password);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Configuration_SetHeadlessMode_ShouldStoreCorrectly(bool headless)
    {
        // Arrange
        var config = new Configuration();

        // Act
        config.HeadlessMode = headless;

        // Assert
        Assert.Equal(headless, config.HeadlessMode);
    }

    [Theory]
    [InlineData(30)]
    [InlineData(120)]
    [InlineData(300)]
    public void Configuration_SetTimeoutSeconds_ShouldStoreCorrectly(int timeout)
    {
        // Arrange
        var config = new Configuration();

        // Act
        config.TimeoutSeconds = timeout;

        // Assert
        Assert.Equal(timeout, config.TimeoutSeconds);
    }

    [Theory]
    [InlineData("CustomAgent/2.0")]
    [InlineData("TestBot/1.5")]
    public void Configuration_SetUserAgent_ShouldStoreCorrectly(string userAgent)
    {
        // Arrange
        var config = new Configuration();

        // Act
        config.UserAgent = userAgent;

        // Assert
        Assert.Equal(userAgent, config.UserAgent);
    }

    [Theory]
    [InlineData("/tmp/report.json")]
    [InlineData("C:\\Reports\\output.json")]
    [InlineData(null)]
    public void Configuration_SetOutputFilePath_ShouldStoreCorrectly(string? outputPath)
    {
        // Arrange
        var config = new Configuration();

        // Act
        config.OutputFilePath = outputPath;

        // Assert
        Assert.Equal(outputPath, config.OutputFilePath);
    }
}