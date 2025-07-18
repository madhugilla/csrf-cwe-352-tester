using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace CSRFTester.Utils;

public class TokenExtractor
{
    private readonly ILogger<TokenExtractor> _logger;

    public TokenExtractor(ILogger<TokenExtractor> logger)
    {
        _logger = logger;
    }

    public Task<List<string>> ExtractTokensAsync(string htmlContent)
    {
        var tokens = new List<string>();

        try
        {
            // Extract tokens from HTML forms (hidden inputs)
            tokens.AddRange(ExtractHiddenFormTokens(htmlContent));
            
            // Extract tokens from meta tags
            tokens.AddRange(ExtractMetaTokens(htmlContent));
            
            // Extract tokens using common patterns
            tokens.AddRange(ExtractTokensByPattern(htmlContent));

            // Remove duplicates and empty tokens
            tokens = tokens.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList();

            _logger.LogDebug("Extracted {TokenCount} CSRF tokens from HTML content", tokens.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting CSRF tokens from HTML content");
        }

        return Task.FromResult(tokens);
    }

    private List<string> ExtractHiddenFormTokens(string htmlContent)
    {
        var tokens = new List<string>();

        try
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            // Common CSRF token field names
            var csrfFieldNames = new[]
            {
                "csrf_token", "csrftoken", "_token", "authenticity_token", 
                "csrf", "_csrf", "csrf_key", "csrf_value", "__token",
                "_csrfToken", "csrfmiddlewaretoken"
            };

            foreach (var fieldName in csrfFieldNames)
            {
                var inputs = doc.DocumentNode.SelectNodes($"//input[@type='hidden' and @name='{fieldName}']");
                if (inputs != null)
                {
                    foreach (var input in inputs)
                    {
                        var value = input.GetAttributeValue("value", "");
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            tokens.Add(value);
                            _logger.LogDebug("Found CSRF token in hidden input '{FieldName}': {Token}", fieldName, value[..Math.Min(value.Length, 10)] + "...");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting hidden form tokens");
        }

        return tokens;
    }

    private List<string> ExtractMetaTokens(string htmlContent)
    {
        var tokens = new List<string>();

        try
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            // Common CSRF meta tag names
            var csrfMetaNames = new[]
            {
                "csrf-token", "csrf_token", "_token", "authenticity-token",
                "csrf", "_csrf", "csrftoken"
            };

            foreach (var metaName in csrfMetaNames)
            {
                var metaTags = doc.DocumentNode.SelectNodes($"//meta[@name='{metaName}']");
                if (metaTags != null)
                {
                    foreach (var meta in metaTags)
                    {
                        var content = meta.GetAttributeValue("content", "");
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            tokens.Add(content);
                            _logger.LogDebug("Found CSRF token in meta tag '{MetaName}': {Token}", metaName, content[..Math.Min(content.Length, 10)] + "...");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting meta tokens");
        }

        return tokens;
    }

    private List<string> ExtractTokensByPattern(string htmlContent)
    {
        var tokens = new List<string>();

        try
        {
            // Common CSRF token patterns in JavaScript or data attributes
            var patterns = new[]
            {
                @"['""]csrf[_-]?token['""]:\s*['""]([^'""]+)['""]",
                @"window\.csrf[_-]?token\s*=\s*['""]([^'""]+)['""]",
                @"data-csrf[_-]?token\s*=\s*['""]([^'""]+)['""]",
                @"authenticity_token['""]:\s*['""]([^'""]+)['""]",
                @"_token['""]:\s*['""]([^'""]+)['""]"
            };

            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(htmlContent, pattern, RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    if (match.Groups.Count > 1)
                    {
                        var token = match.Groups[1].Value;
                        if (!string.IsNullOrWhiteSpace(token) && token.Length > 5)
                        {
                            tokens.Add(token);
                            _logger.LogDebug("Found CSRF token by pattern: {Token}", token[..Math.Min(token.Length, 10)] + "...");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting tokens by pattern");
        }

        return tokens;
    }
}
