using Microsoft.Extensions.Logging;

namespace CSRFTester.Utils;

public class ErrorHandler
{
    private readonly ILogger<ErrorHandler> _logger;

    public ErrorHandler(ILogger<ErrorHandler> logger)
    {
        _logger = logger;
    }

    public void HandleException(Exception ex, string context = "")
    {
        _logger.LogError(ex, "Error in context: {Context}", context);
        
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.ResetColor();
        
        if (!string.IsNullOrEmpty(context))
        {
            Console.WriteLine($"Context: {context}");
        }
    }

    public void HandleWarning(string message, string context = "")
    {
        _logger.LogWarning("Warning in context {Context}: {Message}", context, message);
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Warning: {message}");
        Console.ResetColor();
    }

    public void HandleInfo(string message)
    {
        _logger.LogInformation("{Message}", message);
        Console.WriteLine($"Info: {message}");
    }
}
