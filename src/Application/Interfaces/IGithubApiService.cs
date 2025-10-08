namespace Application.Interfaces;

public interface IGithubApiService
{
    Task<string> CreatePullRequestAsync(string githubToken, string repositoryOwner, string repositoryName, string branchName, string prTitle, string prBody);
}