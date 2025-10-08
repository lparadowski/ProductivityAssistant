namespace Application.Interfaces;

public interface IGithubService
{
    Task PullLatestDevelopAsync(string repositoryPath);

    Task<string> CreateBranchAsync(string ticketTitle, string repositoryPath);

    Task CommitAndPushBranchAsync(string branchName, string title, string description, string repositoryPath);
}