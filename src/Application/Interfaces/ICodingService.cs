namespace Application.Interfaces;

public interface ICodingService
{
    public Task<string> PerformCodeChange(string title, string description, string codebasePath, string repoOwner, string repoName);
}