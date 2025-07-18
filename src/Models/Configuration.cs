namespace CSRFTester.Models;

public class Configuration
{
    public string TargetUrl { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool HeadlessMode { get; set; } = true;
    public bool VerboseLogging { get; set; }
    public string? OutputFilePath { get; set; }
    public int TimeoutSeconds { get; set; } = 60;
    public string UserAgent { get; set; } = "CSRFTester/1.0";
    public string BrowserType { get; set; } = "msedge"; // Always use Microsoft Edge
}
