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
    
    // Azure Entra (Azure AD) Authentication Properties
    public bool UseEntraAuth { get; set; } = false;
    public string? EntraTenantId { get; set; }
    public string? EntraClientId { get; set; }
    public string? EntraRedirectUri { get; set; }
    public string[]? EntraScopes { get; set; }
}
