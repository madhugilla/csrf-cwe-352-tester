using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Invocation;
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

        var useEntraOption = new Option<bool>(
            name: "--use-entra",
            description: "Use Azure Entra (Azure AD) authentication");

        var entraTenantIdOption = new Option<string?>(
            name: "--entra-tenant-id",
            description: "Azure Entra tenant ID (required when using --use-entra)");

        var entraClientIdOption = new Option<string?>(
            name: "--entra-client-id", 
            description: "Azure Entra client/application ID (required when using --use-entra)");

        var entraRedirectUriOption = new Option<string?>(
            name: "--entra-redirect-uri",
            description: "Azure Entra redirect URI (optional, defaults to http://localhost)");

        var entraScopesOption = new Option<string[]?>(
            name: "--entra-scopes",
            description: "Azure Entra scopes (optional, space-separated)");

        // Create root command
        var rootCommand = new RootCommand("CSRF UI Vulnerability Testing Tool - Tests web forms for CWE-352 vulnerabilities")
        {
            urlOption,
            usernameOption,
            passwordOption,
            headlessOption,
            verboseOption,
            outputOption,
            useEntraOption,
            entraTenantIdOption,
            entraClientIdOption,
            entraRedirectUriOption,
            entraScopesOption
        };

        rootCommand.SetHandler(async (InvocationContext context) =>
        {
            try
            {
                var url = context.ParseResult.GetValueForOption(urlOption)!;
                var username = context.ParseResult.GetValueForOption(usernameOption);
                var password = context.ParseResult.GetValueForOption(passwordOption);
                var headless = context.ParseResult.GetValueForOption(headlessOption);
                var verbose = context.ParseResult.GetValueForOption(verboseOption);
                var output = context.ParseResult.GetValueForOption(outputOption);
                var useEntra = context.ParseResult.GetValueForOption(useEntraOption);
                var entraTenantId = context.ParseResult.GetValueForOption(entraTenantIdOption);
                var entraClientId = context.ParseResult.GetValueForOption(entraClientIdOption);
                var entraRedirectUri = context.ParseResult.GetValueForOption(entraRedirectUriOption);
                var entraScopes = context.ParseResult.GetValueForOption(entraScopesOption);

                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                var csrfDetector = serviceProvider.GetRequiredService<CSRFDetector>();
                var resultFormatter = serviceProvider.GetRequiredService<ResultFormatter>();

                // Configure logging level
                if (verbose)
                {
                    logger.LogInformation("Verbose logging enabled");
                }

                // Validate Azure Entra options
                if (useEntra && (string.IsNullOrEmpty(entraTenantId) || string.IsNullOrEmpty(entraClientId)))
                {
                    Console.WriteLine("Error: When using --use-entra, both --entra-tenant-id and --entra-client-id are required.");
                    return;
                }

                // Create configuration
                var config = new Configuration
                {
                    TargetUrl = url,
                    Username = username,
                    Password = password,
                    HeadlessMode = headless,
                    VerboseLogging = verbose,
                    OutputFilePath = output,
                    UseEntraAuth = useEntra,
                    EntraTenantId = entraTenantId,
                    EntraClientId = entraClientId,
                    EntraRedirectUri = entraRedirectUri ?? "http://localhost",
                    EntraScopes = entraScopes
                };

                logger.LogInformation("Starting CSRF UI vulnerability test for: {Url}", url);
                if (useEntra)
                {
                    logger.LogInformation("Using Azure Entra authentication with tenant: {TenantId}", entraTenantId);
                }

                // Run CSRF detection
                var result = await csrfDetector.TestWebPageAsync(config);

                // Format and display results
                await resultFormatter.DisplayResultsAsync(result, config);

                logger.LogInformation("CSRF UI vulnerability test completed");
            }
            catch (Exception ex)
            {
                var verbose = context.ParseResult.GetValueForOption(verboseOption);
                Console.WriteLine($"Error: {ex.Message}");
                if (verbose)
                {
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
                return;
            }
        });

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
