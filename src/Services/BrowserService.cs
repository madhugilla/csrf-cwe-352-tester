using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace CSRFTester.Services;

public class BrowserService : IAsyncDisposable
{
    private readonly ILogger<BrowserService> _logger;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;

    public BrowserService(ILogger<BrowserService> logger)
    {
        _logger = logger;
    }

    public async Task InitializeAsync(string browserType = "msedge", bool headless = true)
    {
        try
        {
            _logger.LogDebug("Initializing Playwright browser: Microsoft Edge, Headless: {Headless}", headless);
            
            _playwright = await Playwright.CreateAsync();
            
            // Always use Microsoft Edge
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions 
            { 
                Headless = headless,
                Channel = "msedge"  // Force use of Microsoft Edge
            });

            _context = await _browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "CSRFTester/1.0",
                ViewportSize = new ViewportSize { Width = 1280, Height = 720 }
            });

            _page = await _context.NewPageAsync();
            
            _logger.LogDebug("Microsoft Edge browser initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Microsoft Edge browser");
            throw;
        }
    }

    public Task<IPage> GetPageAsync()
    {
        if (_page == null)
            throw new InvalidOperationException("Browser not initialized. Call InitializeAsync first.");
        
        return Task.FromResult(_page);
    }

    public async Task NavigateToAsync(string url)
    {
        if (_page == null)
            throw new InvalidOperationException("Browser not initialized. Call InitializeAsync first.");

        _logger.LogDebug("Navigating to: {Url}", url);
        
        var response = await _page.GotoAsync(url, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30000
        });

        if (response == null || !response.Ok)
        {
            throw new Exception($"Failed to navigate to {url}. Status: {response?.Status}");
        }

        _logger.LogDebug("Successfully navigated to: {Url}", url);
    }

    public async Task<List<IElementHandle>> FindFormsAsync()
    {
        if (_page == null)
            throw new InvalidOperationException("Browser not initialized. Call InitializeAsync first.");

        _logger.LogDebug("Finding forms on the page");
        var forms = await _page.QuerySelectorAllAsync("form");
        _logger.LogDebug("Found {FormCount} forms", forms.Count);
        return forms.ToList();
    }

    public async Task<string> GetPageContentAsync()
    {
        if (_page == null)
            throw new InvalidOperationException("Browser not initialized. Call InitializeAsync first.");

        return await _page.ContentAsync();
    }

    public async Task<string> GetPageTitleAsync()
    {
        if (_page == null)
            throw new InvalidOperationException("Browser not initialized. Call InitializeAsync first.");

        return await _page.TitleAsync();
    }

    public Task<string> GetCurrentUrlAsync()
    {
        if (_page == null)
            throw new InvalidOperationException("Browser not initialized. Call InitializeAsync first.");

        return Task.FromResult(_page.Url);
    }

    public async Task FillFormFieldAsync(string selector, string value)
    {
        if (_page == null)
            throw new InvalidOperationException("Browser not initialized. Call InitializeAsync first.");

        _logger.LogDebug("Filling form field: {Selector} with value: {Value}", selector, "***");
        await _page.FillAsync(selector, value);
    }

    public async Task ClickAsync(string selector)
    {
        if (_page == null)
            throw new InvalidOperationException("Browser not initialized. Call InitializeAsync first.");

        _logger.LogDebug("Clicking element: {Selector}", selector);
        await _page.ClickAsync(selector);
    }

    public async Task<IResponse?> SubmitFormAsync(string formSelector)
    {
        if (_page == null)
            throw new InvalidOperationException("Browser not initialized. Call InitializeAsync first.");

        _logger.LogDebug("Submitting form: {FormSelector}", formSelector);
        
        // Wait for navigation after form submission
        var responseTask = _page.WaitForResponseAsync(response => true, new PageWaitForResponseOptions { Timeout = 30000 });
        
        await _page.ClickAsync($"{formSelector} input[type='submit'], {formSelector} button[type='submit'], {formSelector} button:not([type])");
        
        try
        {
            var response = await responseTask;
            _logger.LogDebug("Form submission response: {Status}", response.Status);
            return response;
        }
        catch (TimeoutException)
        {
            _logger.LogWarning("Form submission did not trigger a response within timeout");
            return null;
        }
    }

    public async Task<string?> GetCsrfTokenAsync(IElementHandle form)
    {
        if (_page == null)
            throw new InvalidOperationException("Browser not initialized. Call InitializeAsync first.");

        try
        {
            // Common CSRF token field names
            var csrfSelectors = new[]
            {
                "input[name='csrf_token']",
                "input[name='_token']",
                "input[name='authenticity_token']",
                "input[name='csrfmiddlewaretoken']",
                "input[name='csrf']",
                "input[name='_csrf']"
            };

            foreach (var selector in csrfSelectors)
            {
                var tokenElement = await form.QuerySelectorAsync(selector);
                if (tokenElement != null)
                {
                    var value = await tokenElement.GetAttributeAsync("value");
                    if (!string.IsNullOrEmpty(value))
                    {
                        _logger.LogDebug("Found CSRF token with selector: {Selector}", selector);
                        return value;
                    }
                }
            }

            // Check meta tags
            var metaToken = await _page.GetAttributeAsync("meta[name='csrf-token']", "content");
            if (!string.IsNullOrEmpty(metaToken))
            {
                _logger.LogDebug("Found CSRF token in meta tag");
                return metaToken;
            }

            _logger.LogDebug("No CSRF token found in form");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting CSRF token");
            return null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_page != null)
            {
                await _page.CloseAsync();
                _page = null;
            }

            if (_context != null)
            {
                await _context.CloseAsync();
                _context = null;
            }

            if (_browser != null)
            {
                await _browser.CloseAsync();
                _browser = null;
            }

            _playwright?.Dispose();
            _playwright = null;

            _logger.LogDebug("Browser resources disposed");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error disposing browser resources");
        }
    }
}
