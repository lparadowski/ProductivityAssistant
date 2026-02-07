using Application.Model;

namespace Application.Interfaces;

public interface IClaudeApiService
{
    Task<string[]> DetermineRelevantFilesAsync(string cardTitle, string cardDescription, string[] fileList);

    Task<string> InvestigateWithContextAsync(string cardTitle, string cardDescription, CodeSample[] codeSamples);

    Task<FileChange[]> SuggestCodeChangesWithContextAsync(string cardTitle, string cardDescription, CodeSample[] codeSamples);
}
