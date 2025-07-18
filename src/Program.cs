using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using Microsoft.Playwright;
using CSRFTester.Services;
using CSRFTester.Models;
using CSRFTester.Utils;

namespace CSRFTester;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Configure services
        var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();

        // Configure command line options
        var urlOption = new Option<string>(
            name: "--url",
            description: "The target web page URL to test for CSRF vulnerabilities")
        {
            IsRequired = true
        };

        var usernameOption = new Option<string?>(
            name: "--username",
            description: "Username for authentication (if required)");

        var passwordOption = new Option<string?>(
            name: "--password",
            description: "Password for authentication (if required)");

        var headlessOption = new Option<bool>(
            name: "--headless",
            description: "Run browser in headless mode (default: true)");
        headlessOption.SetDefaultValue(true);

        var verboseOption = new Option<bool>(
            name: "--verbose",
            description: "Enable verbose logging");

        var outputOption = new Option<string?>(
            name: "--output",
            description: "Output file path for detailed report (optional)");

        // Create root command
        var rootCommand = new RootCommand("CSRF UI Vulnerability Testing Tool - Tests web forms for CWE-352 vulnerabilities")
        {
            urlOption,
            usernameOption,
            passwordOption,
            headlessOption,
            verboseOption,
            outputOption
        };

        rootCommand.SetHandler(async (url, username, password, headless, verbose, output) =>
        {
            try
            {
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                var csrfDetector = serviceProvider.GetRequiredService<CSRFDetector>();
                var resultFormatter = serviceProvider.GetRequiredService<ResultFormatter>();

                // Configure logging level
                if (verbose)
                {
                    logger.LogInformation("Verbose logging enabled");
                }

                // Create configuration
                var config = new Configuration
                {
                    TargetUrl = url,
                    Username = username,
                    Password = password,
                    HeadlessMode = headless,
                    VerboseLogging = verbose,
                    OutputFilePath = output
                };

                logger.LogInformation("Starting CSRF UI vulnerability test for: {Url}", url);

                // Run CSRF detection
                var result = await csrfDetector.TestWebPageAsync(config);

                // Format and display results
                await resultFormatter.DisplayResultsAsync(result, config);

                logger.LogInformation("CSRF UI vulnerability test completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                if (verbose)
                {
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
                return;
            }
        }, urlOption, usernameOption, passwordOption, headlessOption, verboseOption, outputOption);

        return await rootCommand.InvokeAsync(args);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Add HTTP client (still needed for some operations)
        services.AddHttpClient<HttpClientService>();

        // Add browser service
        services.AddTransient<BrowserService>();

        // Add application services
        services.AddTransient<CSRFDetector>();
        services.AddTransient<CSRFTestRunner>();
        services.AddTransient<AuthenticationService>();
        services.AddTransient<TokenExtractor>();
        services.AddTransient<ResultFormatter>();
        services.AddTransient<ErrorHandler>();
    }
}
