using CSRFTester.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using Xunit;

namespace CSRFTester.Tests.Services;

public class HttpClientServiceTests
{
    private readonly Mock<ILogger<HttpClientService>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly HttpClientService _httpClientService;

    public HttpClientServiceTests()
    {
        _mockLogger = new Mock<ILogger<HttpClientService>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _httpClientService = new HttpClientService(_httpClient, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_ShouldSetDefaultConfiguration()
    {
        // Act
        var service = new HttpClientService(new HttpClient(), _mockLogger.Object);

        // Assert
        Assert.Equal(TimeSpan.FromSeconds(60), _httpClient.Timeout);
        Assert.Contains("CSRFTester/1.0", _httpClient.DefaultRequestHeaders.UserAgent.ToString());
    }

    [Fact]
    public async Task GetAsync_ValidUrl_ShouldReturnResponse()
    {
        // Arrange
        var url = "https://example.com/test";
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("Test response")
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri!.ToString() == url),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);

        // Act
        var response = await _httpClientService.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("Test response", content);
    }

    [Fact]
    public async Task PostAsync_ValidUrlAndContent_ShouldReturnResponse()
    {
        // Arrange
        var url = "https://example.com/submit";
        var content = new StringContent("test data");
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.Created);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri!.ToString() == url),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);

        // Act
        var response = await _httpClientService.PostAsync(url, content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task SendAsync_CustomRequest_ShouldReturnResponse()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Put, "https://example.com/update");
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);

        // Act
        var response = await _httpClientService.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public void SetAuthenticationHeader_ValidSchemeAndToken_ShouldSetHeader()
    {
        // Arrange
        var scheme = "Bearer";
        var token = "test-token-123";

        // Act
        _httpClientService.SetAuthenticationHeader(scheme, token);

        // Assert
        Assert.Equal(scheme, _httpClient.DefaultRequestHeaders.Authorization?.Scheme);
        Assert.Equal(token, _httpClient.DefaultRequestHeaders.Authorization?.Parameter);
    }

    [Fact]
    public void AddDefaultHeader_ValidNameAndValue_ShouldAddHeader()
    {
        // Arrange
        var headerName = "X-Custom-Header";
        var headerValue = "custom-value";

        // Act
        _httpClientService.AddDefaultHeader(headerName, headerValue);

        // Assert
        Assert.True(_httpClient.DefaultRequestHeaders.Contains(headerName));
        Assert.Equal(headerValue, _httpClient.DefaultRequestHeaders.GetValues(headerName).First());
    }

    [Fact]
    public async Task GetAsync_NetworkError_ShouldThrowException()
    {
        // Arrange
        var url = "https://invalid-domain.com/test";
        
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _httpClientService.GetAsync(url));
    }

    [Fact]
    public async Task PostAsync_NetworkError_ShouldThrowException()
    {
        // Arrange
        var url = "https://invalid-domain.com/submit";
        var content = new StringContent("test data");
        
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _httpClientService.PostAsync(url, content));
    }

    [Theory]
    [InlineData("Basic")]
    [InlineData("Bearer")]
    [InlineData("ApiKey")]
    public void SetAuthenticationHeader_DifferentSchemes_ShouldSetCorrectly(string scheme)
    {
        // Arrange
        var token = "test-token";

        // Act
        _httpClientService.SetAuthenticationHeader(scheme, token);

        // Assert
        Assert.Equal(scheme, _httpClient.DefaultRequestHeaders.Authorization?.Scheme);
        Assert.Equal(token, _httpClient.DefaultRequestHeaders.Authorization?.Parameter);
    }

    [Fact]
    public void AddDefaultHeader_MultipleHeaders_ShouldAddAll()
    {
        // Arrange & Act
        _httpClientService.AddDefaultHeader("X-Header-1", "value1");
        _httpClientService.AddDefaultHeader("X-Header-2", "value2");
        _httpClientService.AddDefaultHeader("X-Header-3", "value3");

        // Assert
        Assert.True(_httpClient.DefaultRequestHeaders.Contains("X-Header-1"));
        Assert.True(_httpClient.DefaultRequestHeaders.Contains("X-Header-2"));
        Assert.True(_httpClient.DefaultRequestHeaders.Contains("X-Header-3"));
        Assert.Equal("value1", _httpClient.DefaultRequestHeaders.GetValues("X-Header-1").First());
        Assert.Equal("value2", _httpClient.DefaultRequestHeaders.GetValues("X-Header-2").First());
        Assert.Equal("value3", _httpClient.DefaultRequestHeaders.GetValues("X-Header-3").First());
    }
}