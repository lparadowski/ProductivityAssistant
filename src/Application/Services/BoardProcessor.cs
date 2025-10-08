using Application.Dtos;
using Application.Interfaces;
using Application.Settings;

namespace Application.Services;

public class BoardProcessor(ITrelloService trelloService, IInvestigationService investigationService, ICodingService codingService, ApplicationSettings applicationSettings) : IBoardProcessor
{
    public async Task ProcessBoardAsync(string boardId)
    {
        var todoCards = await trelloService.GetTodoCardsAsync(boardId);

        //Hardcoded Labels for now, could be made configurable in the future.
        var investigationCards = todoCards.Where(c => c.Labels.Contains("Investigation"));
        var simpleCards = todoCards.Where(c => c.Labels.Contains("Simple"));

        foreach (var card in investigationCards)
        {
            // Skip cards that already have AI investigation comments.
            if (await trelloService.HasAIInvestigationCommentAsync(card.Id))
            {
                continue;
            }

            // Resolve codebase path from card labels
            var codebasePath = ResolveCodebasePath(card.Labels);
            if (string.IsNullOrEmpty(codebasePath))
            {
                continue;
            }

            var investigation = await investigationService.InvestigateAsync(card.Name, card.Description, codebasePath);

            // Post investigation results as comment to Trello card
            await trelloService.PostCommentAsync(card.Id, investigation);
        }

        foreach (var card in simpleCards)
        {
            // Skip cards that already have productivity agent comments
            if (await trelloService.HasProductivityCommentAsync(card.Id))
            {
                continue;
            }

            // Resolve codebase path and GitHub repo from card labels
            var codebasePath = ResolveCodebasePath(card.Labels);
            var githubRepo = ResolveGitHubRepository(card.Labels);

            if (string.IsNullOrEmpty(codebasePath) || githubRepo == null)
            {
                continue;
            }

            var prUrl = await codingService.PerformCodeChange(card.Name, card.Description, codebasePath, githubRepo.Owner, githubRepo.Name);

            // Post PR URL as comment to Trello card
            var comment = $"Productivity agent created this pull request: {prUrl}";
            await trelloService.PostCommentAsync(card.Id, comment);
        }
    }

    private string ResolveCodebasePath(List<string> cardLabels)
    {
        foreach (var label in cardLabels)
        {
            if (applicationSettings.CodebaseMappings.TryGetValue(label, out var codebasePath))
            {
                return codebasePath;
            }
        }

        return string.Empty;
    }

    private GitHubRepository? ResolveGitHubRepository(List<string> cardLabels)
    {
        foreach (var label in cardLabels)
        {
            if (applicationSettings.GitHubRepositories.TryGetValue(label, out var githubRepo))
            {
                return githubRepo;
            }
        }

        return null;
    }
}
