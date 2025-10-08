namespace Application.Model;

public class CodeSample
{
    public string FilePath { get; set; } = string.Empty;       // "src/Application/Services/MappingProfile.cs"
    public string Section { get; set; } = string.Empty;        // Just the relevant method/class/config block
    public string SectionType { get; set; } = string.Empty;    // "class", "method", "configuration", etc.
    public int LineNumber { get; set; }                        // Where this section starts in file
    public int RelevanceScore { get; set; }                    // 1-10 priority for including in API call
    public string MatchReason { get; set; } = string.Empty;    // "Matched pattern *Mapping*.cs, contains 'AutoMapper'"
}