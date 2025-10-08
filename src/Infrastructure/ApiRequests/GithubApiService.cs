using Application.Interfaces;
using Octokit;

namespace Infrastructure.ApiRequests;

public class GithubApiService : IGithubApiService
{
    public async Task<string> CreatePullRequestAsync(string githubToken, string repositoryOwner, string repositoryName, string branchName, string prTitle, string prBody)
    {
        var client = new GitHubClient(new ProductHeaderValue("AI-Assistant"))
        {
            Credentials = new Credentials(githubToken)
        };

        var newPullRequest = new NewPullRequest(prTitle, branchName, "develop")
        {
            Body = prBody
        };

        try
        {
            var pullRequest = await client.PullRequest.Create(repositoryOwner, repositoryName, newPullRequest);
            return pullRequest.HtmlUrl;
        }
        catch (ApiValidationException ex)
        {
            var errors = string.Join(", ", ex.ApiError.Errors?.Select(e => $"{e.Field}: {e.Message}") ?? new[] { "Unknown validation error" });
            throw new InvalidOperationException($"GitHub validation error: {errors}. PR: '{prTitle}' from '{branchName}' to 'develop'", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create pull request '{prTitle}' from '{branchName}' to 'develop': {ex.Message}", ex);
        }
    }
}