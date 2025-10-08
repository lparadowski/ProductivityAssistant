namespace Application.Model;

public class CodeChangeTopics
{
    public string[] Entities { get; set; } = [];        // ["Invoice", "Customer"]
    public string[] Operations { get; set; } = [];      // ["delete", "create", "update"]
    public string[] Layers { get; set; } = [];          // ["API", "Service", "Repository", "Domain"]
    public string[] FilePatterns { get; set; } = [];    // ["*Invoice*.cs", "*Customer*.cs"]
}