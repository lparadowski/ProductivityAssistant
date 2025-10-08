namespace Application.Model;

public class InvestigationTopics
{
    public string[] Technologies { get; set; } = [];      // ["AutoMapper", "Redis", "Entity Framework"]
    public string[] Actions { get; set; } = [];           // ["investigate", "replace", "implement", "analyze"]
    public string[] FilePatterns { get; set; } = [];      // ["*Mapping*.cs", "*Cache*.cs", "*Repository*.cs"]
}