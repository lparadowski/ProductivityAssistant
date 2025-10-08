using Application.Model;

namespace Application.Interfaces;

public interface ICodeSampleLocator
{
    Task<CodeSample[]> FindRelevantCodeAsync(InvestigationTopics topics, string codebasePath);

    Task<CodeSample[]> FindRelevantCodeAsync(CodeChangeTopics topics, string codebasePath);
}