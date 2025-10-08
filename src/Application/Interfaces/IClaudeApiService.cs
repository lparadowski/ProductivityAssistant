using Application.Model;

namespace Application.Interfaces;

public interface IClaudeApiService
{
    Task<InvestigationTopics> ExtractTopicsAsync(string cardTitle, string cardDescription);

    Task<CodeChangeTopics> ExtractCodeChangeTopicsAsync(string cardTitle, string cardDescription);

    Task<string> InvestigateWithContextAsync(string cardTitle, string cardDescription, CodeSample[] codeSamples);

    Task<FileChange[]> SuggestCodeChangesWithContextAsync(string cardTitle, string cardDescription, CodeSample[] codeSamples);
}