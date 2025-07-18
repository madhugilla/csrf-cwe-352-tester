using Microsoft.Extensions.Logging;

namespace CSRFTester.Services;

public class HttpClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpClientService> _logger;

    public HttpClientService(HttpClient httpClient, ILogger<HttpClientService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        // Configure default settings
        _httpClient.Timeout = TimeSpan.FromSeconds(60);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "CSRFTester/1.0");
    }

    public async Task<HttpResponseMessage> GetAsync(string url)
    {
        _logger.LogDebug("Making GET request to: {Url}", url);
        var response = await _httpClient.GetAsync(url);
        _logger.LogDebug("GET response status: {StatusCode}", response.StatusCode);
        return response;
    }

    public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
    {
        _logger.LogDebug("Making POST request to: {Url}", url);
        var response = await _httpClient.PostAsync(url, content);
        _logger.LogDebug("POST response status: {StatusCode}", response.StatusCode);
        return response;
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        _logger.LogDebug("Making {Method} request to: {Url}", request.Method, request.RequestUri);
        var response = await _httpClient.SendAsync(request);
        _logger.LogDebug("{Method} response status: {StatusCode}", request.Method, response.StatusCode);
        return response;
    }

    public void SetAuthenticationHeader(string scheme, string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(scheme, token);
        _logger.LogDebug("Set authentication header: {Scheme}", scheme);
    }

    public void AddDefaultHeader(string name, string value)
    {
        _httpClient.DefaultRequestHeaders.Add(name, value);
        _logger.LogDebug("Added default header: {Name} = {Value}", name, value);
    }
}
