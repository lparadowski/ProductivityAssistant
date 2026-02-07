using Application.Interfaces;
using Application.Settings;

namespace Application.Services;

public class CodingService(IGithubService githubService, IClaudeApiService claudeApiService, ICodeSampleLocator codeSampleLocator, ICodeChangeApplicator codeChangeApplicator, IGithubApiService githubApiService, ApplicationSettings settings) : ICodingService
{
    public async Task<string> PerformCodeChange(string title, string description, string codebasePath, string repoOwner, string repoName)
    {
        await githubService.PullLatestDevelopAsync(codebasePath);
        var branchName = await githubService.CreateBranchAsync(title, codebasePath);

        var fileList = await codeSampleLocator.GetFilteredFileListAsync(codebasePath);
        var relevantFiles = await claudeApiService.DetermineRelevantFilesAsync(title, description, fileList);
        var codeSamples = await codeSampleLocator.GetCodeSamplesForFilesAsync(relevantFiles, codebasePath);

        var suggestedChanges = await claudeApiService.SuggestCodeChangesWithContextAsync(title, description, codeSamples);

        await codeChangeApplicator.ApplyChanges(suggestedChanges, codebasePath);

        await githubService.CommitAndPushBranchAsync(branchName, title, description, codebasePath);
        var pullRequestUrl = await githubApiService.CreatePullRequestAsync(settings.GitHubToken, repoOwner, repoName, branchName, title, description);

        return pullRequestUrl;
    }
}
