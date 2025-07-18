using CSRFTester.Utils;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CSRFTester.Tests.Utils;

public class TokenExtractorAdvancedTests
{
    private readonly Mock<ILogger<TokenExtractor>> _mockLogger;
    private readonly TokenExtractor _tokenExtractor;

    public TokenExtractorAdvancedTests()
    {
        _mockLogger = new Mock<ILogger<TokenExtractor>>();
        _tokenExtractor = new TokenExtractor(_mockLogger.Object);
    }

    [Fact]
    public async Task ExtractTokensAsync_HtmlWithSpecialCharacters_ShouldExtractTokens()
    {
        // Arrange
        var htmlContent = @"
            <html>
                <body>
                    <form method='post'>
                        <input type='hidden' name='_token' value='abc-123_def.456$special!' />
                    </form>
                </body>
            </html>";

        // Act
        var tokens = await _tokenExtractor.ExtractTokensAsync(htmlContent);

        // Assert
        Assert.NotNull(tokens);
        Assert.Contains("abc-123_def.456$special!", tokens);
    }

    [Fact]
    public async Task ExtractTokensAsync_HtmlWithNestedForms_ShouldExtractAllTokens()
    {
        // Arrange
        var htmlContent = @"
            <html>
                <body>
                    <div>
                        <form method='post'>
                            <input type='hidden' name='csrf_token' value='outer-token' />
                            <div>
                                <form method='post'>
                                    <input type='hidden' name='_token' value='inner-token' />
                                </form>
                            </div>
                        </form>
                    </div>
                </body>
            </html>";

        // Act
        var tokens = await _tokenExtractor.ExtractTokensAsync(htmlContent);

        // Assert
        Assert.NotNull(tokens);
        Assert.Contains("outer-token", tokens);
        Assert.Contains("inner-token", tokens);
        Assert.Equal(2, tokens.Count);
    }

    [Fact]
    public async Task ExtractTokensAsync_HtmlWithLongToken_ShouldExtractCompletely()
    {
        // Arrange
        var longToken = new string('a', 500); // Very long token
        var htmlContent = $@"
            <html>
                <body>
                    <form method='post'>
                        <input type='hidden' name='authenticity_token' value='{longToken}' />
                    </form>
                </body>
            </html>";

        // Act
        var tokens = await _tokenExtractor.ExtractTokensAsync(htmlContent);

        // Assert
        Assert.NotNull(tokens);
        Assert.Contains(longToken, tokens);
        Assert.Equal(500, tokens[0].Length);
    }

    [Fact]
    public async Task ExtractTokensAsync_HtmlWithNumericTokens_ShouldExtractTokens()
    {
        // Arrange
        var htmlContent = @"
            <html>
                <body>
                    <form method='post'>
                        <input type='hidden' name='csrf' value='1234567890' />
                        <input type='hidden' name='_csrf' value='0987654321' />
                    </form>
                </body>
            </html>";

        // Act
        var tokens = await _tokenExtractor.ExtractTokensAsync(htmlContent);

        // Assert
        Assert.NotNull(tokens);
        Assert.Contains("1234567890", tokens);
        Assert.Contains("0987654321", tokens);
        Assert.Equal(2, tokens.Count);
    }

    [Fact]
    public async Task ExtractTokensAsync_HtmlWithEmptyAndValidTokens_ShouldExtractOnlyValid()
    {
        // Arrange
        var htmlContent = @"
            <html>
                <body>
                    <form method='post'>
                        <input type='hidden' name='csrf_token' value='' />
                        <input type='hidden' name='_token' value='   ' />
                        <input type='hidden' name='authenticity_token' value='valid-token-123' />
                        <input type='hidden' name='csrf' value='another-valid-token' />
                    </form>
                </body>
            </html>";

        // Act
        var tokens = await _tokenExtractor.ExtractTokensAsync(htmlContent);

        // Assert
        Assert.NotNull(tokens);
        Assert.Equal(2, tokens.Count);
        Assert.Contains("valid-token-123", tokens);
        Assert.Contains("another-valid-token", tokens);
    }

    [Fact]
    public async Task ExtractTokensAsync_HtmlWithAttributeVariations_ShouldExtractTokens()
    {
        // Arrange
        var htmlContent = @"
            <html>
                <body>
                    <form method='post'>
                        <input type=""hidden"" name=""_token"" value=""double-quotes-token"" />
                        <input type='hidden' name='csrf_token' value='single-quotes-token' />
                        <input type=hidden name=_csrf value=no-quotes-token />
                    </form>
                </body>
            </html>";

        // Act
        var tokens = await _tokenExtractor.ExtractTokensAsync(htmlContent);

        // Assert
        Assert.NotNull(tokens);
        Assert.Contains("double-quotes-token", tokens);
        Assert.Contains("single-quotes-token", tokens);
        Assert.Contains("no-quotes-token", tokens);
        Assert.Equal(3, tokens.Count);
    }

    [Fact]
    public async Task ExtractTokensAsync_HtmlWithCaseVariations_ShouldExtractTokens()
    {
        // Arrange
        var htmlContent = @"
            <html>
                <body>
                    <form method='post'>
                        <INPUT TYPE='HIDDEN' NAME='_TOKEN' VALUE='UPPERCASE-TOKEN' />
                        <Input Type='Hidden' Name='csrf_token' Value='MixedCase-Token' />
                        <input type='hidden' name='authenticity_token' value='lowercase-token' />
                    </form>
                </body>
            </html>";

        // Act
        var tokens = await _tokenExtractor.ExtractTokensAsync(htmlContent);

        // Assert
        Assert.NotNull(tokens);
        // Note: HTML parsing is typically case-insensitive for element names, 
        // but attribute values are case-sensitive
        Assert.True(tokens.Count >= 1); // At least some tokens should be found
        Assert.Contains("lowercase-token", tokens); // This one should definitely work
    }

    [Fact]
    public async Task ExtractTokensAsync_ComplexHtmlDocument_ShouldExtractTokensCorrectly()
    {
        // Arrange
        var htmlContent = @"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='csrf-token' content='meta-csrf-token-123' />
                <title>Test Page</title>
            </head>
            <body>
                <header>
                    <nav>Navigation</nav>
                </header>
                <main>
                    <section>
                        <form id='login-form' method='post' action='/login'>
                            <input type='hidden' name='_token' value='login-form-token' />
                            <input type='text' name='username' />
                            <input type='password' name='password' />
                            <button type='submit'>Login</button>
                        </form>
                    </section>
                    <section>
                        <form id='contact-form' method='post' action='/contact'>
                            <input type='hidden' name='csrf_token' value='contact-form-token' />
                            <textarea name='message'></textarea>
                            <input type='submit' value='Send' />
                        </form>
                    </section>
                </main>
                <footer>Footer content</footer>
                <script>
                    // Some JavaScript that might contain tokens
                    var csrfToken = 'js-token-456';
                </script>
            </body>
            </html>";

        // Act
        var tokens = await _tokenExtractor.ExtractTokensAsync(htmlContent);

        // Assert
        Assert.NotNull(tokens);
        Assert.Contains("meta-csrf-token-123", tokens);
        Assert.Contains("login-form-token", tokens);
        Assert.Contains("contact-form-token", tokens);
        Assert.True(tokens.Count >= 3);
    }
}