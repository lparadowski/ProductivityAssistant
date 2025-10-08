namespace Application.Settings;

public class ApplicationSettings
{
    public int MaxRetryDelayInSeconds { get; set; }
    public int ExecutionIntervalInSeconds { get; set; }
    public string TrelloApiKey { get; set; } = string.Empty;
    public string TrelloApiToken { get; set; } = string.Empty;
    public string[] TrelloBoardIds { get; set; } = Array.Empty<string>();
    public Dictionary<string, string> CodebaseMappings { get; set; } = new();
    public Dictionary<string, GitHubRepository> GitHubRepositories { get; set; } = new();
    public string AnthropicApiKey { get; set; } = string.Empty;
    public string GitHubToken { get; set; } = string.Empty;
}

public class GitHubRepository
{
    public string Owner { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}