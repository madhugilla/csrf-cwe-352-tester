using Microsoft.Extensions.Logging;
using CSRFTester.Models;
using Newtonsoft.Json;

namespace CSRFTester.Utils;

public class ResultFormatter
{
    private readonly ILogger<ResultFormatter> _logger;

    public ResultFormatter(ILogger<ResultFormatter> logger)
    {
        _logger = logger;
    }

    public async Task DisplayResultsAsync(VulnerabilityResult result, Configuration config)
    {
        try
        {
            // Display console output
            DisplayConsoleResults(result, config);

            // Save to file if requested
            if (!string.IsNullOrEmpty(config.OutputFilePath))
            {
                await SaveResultsToFileAsync(result, config.OutputFilePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error displaying results");
            Console.WriteLine($"Error displaying results: {ex.Message}");
        }
    }

    private void DisplayConsoleResults(VulnerabilityResult result, Configuration config)
    {
        Console.WriteLine();
        Console.WriteLine(new string('=', 80));
        Console.WriteLine("CSRF VULNERABILITY TEST RESULTS");
        Console.WriteLine(new string('=', 80));
        Console.WriteLine();
        
        Console.WriteLine($"Target URL: {result.TargetUrl}");
        Console.WriteLine($"Test Date: {result.TestDate:yyyy-MM-dd HH:mm:ss} UTC");
        Console.WriteLine();

        // Display individual test results
        Console.WriteLine("TEST RESULTS:");
        Console.WriteLine(new string('-', 40));
        
        foreach (var test in result.TestResults)
        {
            var status = test.Passed ? "✓ PASS" : "✗ FAIL";
            var statusColor = test.Passed ? ConsoleColor.Green : ConsoleColor.Red;
            
            Console.Write($"{status.PadRight(8)} ");
            Console.ForegroundColor = statusColor;
            Console.Write($"[{test.Severity.ToString().ToUpper()}]".PadRight(10));
            Console.ResetColor();
            Console.WriteLine($" {test.TestName}");
            
            if (config.VerboseLogging || !test.Passed)
            {
                Console.WriteLine($"         Description: {test.Description}");
                if (!string.IsNullOrEmpty(test.Details))
                {
                    Console.WriteLine($"         Details: {test.Details}");
                }
                Console.WriteLine();
            }
        }

        // Overall verdict
        Console.WriteLine();
        Console.WriteLine("OVERALL ASSESSMENT:");
        Console.WriteLine(new string('-', 40));
        
        if (result.IsVulnerable)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("🚨 VULNERABLE");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ SECURE");
        }
        Console.ResetColor();
        
        Console.WriteLine($"Summary: {result.Summary}");
        Console.WriteLine();

        // Recommendations
        if (result.Recommendations.Any())
        {
            Console.WriteLine("RECOMMENDATIONS:");
            Console.WriteLine(new string('-', 40));
            foreach (var recommendation in result.Recommendations)
            {
                Console.WriteLine($"• {recommendation}");
            }
            Console.WriteLine();
        }

        Console.WriteLine(new string('=', 80));
    }

    private async Task SaveResultsToFileAsync(VulnerabilityResult result, string filePath)
    {
        try
        {
            var jsonResult = JsonConvert.SerializeObject(result, Formatting.Indented);
            await File.WriteAllTextAsync(filePath, jsonResult);
            
            _logger.LogInformation("Results saved to file: {FilePath}", filePath);
            Console.WriteLine($"Detailed results saved to: {filePath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving results to file: {FilePath}", filePath);
            Console.WriteLine($"Error saving results to file: {ex.Message}");
        }
    }
}
