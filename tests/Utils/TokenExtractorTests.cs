using CSRFTester.Utils;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CSRFTester.Tests.Utils;

public class TokenExtractorTests
{
    private readonly Mock<ILogger<TokenExtractor>> _mockLogger;
    private readonly TokenExtractor _tokenExtractor;

    public TokenExtractorTests()
    {
        _mockLogger = new Mock<ILogger<TokenExtractor>>();
        _tokenExtractor = new TokenExtractor(_mockLogger.Object);
    }

    [Fact]
    public async Task ExtractTokensAsync_EmptyHtml_ShouldReturnEmptyList()
    {
        // Arrange
        var htmlContent = string.Empty;

        // Act
        var tokens = await _tokenExtractor.ExtractTokensAsync(htmlContent);

        // Assert
        Assert.NotNull(tokens);
        Assert.Empty(tokens);
    }

    [Fact]
    public async Task ExtractTokensAsync_HtmlWithHiddenCsrfToken_ShouldExtractToken()
    {
        // Arrange
        var htmlContent = @"
            <html>
                <body>
                    <form method='post'>
                        <input type='hidden' name='_token' value='abc123def456' />
                        <input type='text' name='username' />
                        <input type='submit' value='Submit' />
                    </form>
                </body>
            </html>";

        // Act
        var tokens = await _tokenExtractor.ExtractTokensAsync(htmlContent);

        // Assert
        Assert.NotNull(tokens);
        Assert.Contains("abc123def456", tokens);
    }

    [Fact]
    public async Task ExtractTokensAsync_HtmlWithCsrfTokenInput_ShouldExtractToken()
    {
        // Arrange
        var htmlContent = @"
            <html>
                <body>
                    <form method='post'>
                        <input type='hidden' name='csrf_token' value='xyz789uvw' />
                        <input type='text' name='email' />
                    </form>
                </body>
            </html>";

        // Act
        var tokens = await _tokenExtractor.ExtractTokensAsync(htmlContent);

        // Assert
        Assert.NotNull(tokens);
        Assert.Contains("xyz789uvw", tokens);
    }

    [Fact]
    public async Task ExtractTokensAsync_HtmlWithMetaCsrfToken_ShouldExtractToken()
    {
        // Arrange
        var htmlContent = @"
            <html>
                <head>
                    <meta name='csrf-token' content='meta123token456' />
                </head>
                <body>
                    <form method='post'>
                        <input type='text' name='data' />
                    </form>
                </body>
            </html>";

        // Act
        var tokens = await _tokenExtractor.ExtractTokensAsync(htmlContent);

        // Assert
        Assert.NotNull(tokens);
        Assert.Contains("meta123token456", tokens);
    }

    [Fact]
    public async Task ExtractTokensAsync_HtmlWithMultipleTokens_ShouldExtractAllUniqueTokens()
    {
        // Arrange
        var htmlContent = @"
            <html>
                <head>
                    <meta name='csrf-token' content='token1' />
                </head>
                <body>
                    <form method='post'>
                        <input type='hidden' name='_token' value='token2' />
                        <input type='hidden' name='authenticity_token' value='token3' />
                        <input type='hidden' name='_token' value='token2' />
                    </form>
                </body>
            </html>";

        // Act
        var tokens = await _tokenExtractor.ExtractTokensAsync(htmlContent);

        // Assert
        Assert.NotNull(tokens);
        Assert.Equal(3, tokens.Count);
        Assert.Contains("token1", tokens);
        Assert.Contains("token2", tokens);
        Assert.Contains("token3", tokens);
    }

    [Theory]
    [InlineData("_token")]
    [InlineData("csrf_token")]
    [InlineData("authenticity_token")]
    [InlineData("csrfmiddlewaretoken")]
    public async Task ExtractTokensAsync_HtmlWithCommonTokenNames_ShouldExtractTokens(string tokenName)
    {
        // Arrange
        var tokenValue = $"value_{tokenName}_123";
        var htmlContent = $@"
            <html>
                <body>
                    <form method='post'>
                        <input type='hidden' name='{tokenName}' value='{tokenValue}' />
                    </form>
                </body>
            </html>";

        // Act
        var tokens = await _tokenExtractor.ExtractTokensAsync(htmlContent);

        // Assert
        Assert.NotNull(tokens);
        Assert.Contains(tokenValue, tokens);
    }

    [Fact]
    public async Task ExtractTokensAsync_HtmlWithWhitespaceTokens_ShouldFilterOutEmptyTokens()
    {
        // Arrange
        var htmlContent = @"
            <html>
                <body>
                    <form method='post'>
                        <input type='hidden' name='_token' value='validtoken' />
                        <input type='hidden' name='empty_token' value='' />
                        <input type='hidden' name='whitespace_token' value='   ' />
                    </form>
                </body>
            </html>";

        // Act
        var tokens = await _tokenExtractor.ExtractTokensAsync(htmlContent);

        // Assert
        Assert.NotNull(tokens);
        Assert.Single(tokens);
        Assert.Contains("validtoken", tokens);
    }

    [Fact]
    public async Task ExtractTokensAsync_MalformedHtml_ShouldHandleGracefully()
    {
        // Arrange
        var htmlContent = @"
            <html>
                <body>
                    <form method='post'>
                        <input type='hidden' name='_token' value='token123'
                        <input type='text' name='data' />
                    </form>
                </body>";

        // Act
        var result = await _tokenExtractor.ExtractTokensAsync(htmlContent);

        // Assert
        Assert.NotNull(result);
        // Should still extract the token despite malformed HTML
        Assert.Contains("token123", result);
    }

    [Fact]
    public async Task ExtractTokensAsync_ExtremelyLargeHtml_ShouldHandleGracefully()
    {
        // Arrange
        var largeContent = new string('x', 10000); // Large content
        var htmlContent = $@"
            <html>
                <body>
                    <!-- Large content: {largeContent} -->
                    <form method='post'>
                        <input type='hidden' name='_token' value='embedded_token_123' />
                    </form>
                </body>
            </html>";

        // Act
        var tokens = await _tokenExtractor.ExtractTokensAsync(htmlContent);

        // Assert
        Assert.NotNull(tokens);
        Assert.Contains("embedded_token_123", tokens);
    }

    [Fact]
    public async Task ExtractTokensAsync_HtmlWithoutForms_ShouldReturnEmptyList()
    {
        // Arrange
        var htmlContent = @"
            <html>
                <head>
                    <title>Test Page</title>
                </head>
                <body>
                    <h1>Welcome</h1>
                    <p>This page has no forms.</p>
                </body>
            </html>";

        // Act
        var tokens = await _tokenExtractor.ExtractTokensAsync(htmlContent);

        // Assert
        Assert.NotNull(tokens);
        Assert.Empty(tokens);
    }

    [Fact]
    public async Task ExtractTokensAsync_HtmlWithJavaScriptTokenPattern_ShouldExtractTokens()
    {
        // Arrange
        var htmlContent = @"
            <html>
                <head>
                    <script>
                        window.csrf_token = 'js_csrf_token_123456';
                        var authenticity_token = 'auth_token_789012';
                        var config = {
                            'csrf-token': 'config_csrf_345678',
                            '_token': 'underscore_token_901234'
                        };
                    </script>
                </head>
                <body>
                    <div data-csrf-token='data_attribute_token_567890'></div>
                    <form method='post'></form>
                </body>
            </html>";

        // Act
        var tokens = await _tokenExtractor.ExtractTokensAsync(htmlContent);

        // Assert
        Assert.NotNull(tokens);
        // Should extract tokens from JavaScript patterns
        Assert.True(tokens.Count > 0);
    }
}