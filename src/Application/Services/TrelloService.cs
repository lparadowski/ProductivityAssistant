using Application.Dtos;
using Application.Interfaces;
using Application.Settings;
using Manatee.Trello;

namespace Application.Services;

public class TrelloService(ApplicationSettings applicationSettings) : ITrelloService
{
    public async Task<IEnumerable<TrelloCardDto>> GetTodoCardsAsync(string boardId)
    {
        TrelloAuthorization.Default.AppKey = applicationSettings.TrelloApiKey;
        TrelloAuthorization.Default.UserToken = applicationSettings.TrelloApiToken;

        try
        {
            var board = new Board(boardId);
            await board.Refresh();

            // Find the "Todo" or "To Do" list (case-insensitive), there might be a better way to do this.
            var todoList = board.Lists.FirstOrDefault(l =>
                string.Equals(l.Name, "Todo", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(l.Name, "To Do", StringComparison.OrdinalIgnoreCase));

            if (todoList == null)
            {
                throw new InvalidOperationException("Todo list not found on the board");
            }

            await todoList.Cards.Refresh();

            var cardDtos = new List<TrelloCardDto>();

            foreach (var card in todoList.Cards)
            {
                //Get full card details.
                await card.Refresh();

                foreach (var label in card.Labels!)
                {
                    Console.WriteLine($"  Label: {label.Name}, Color: {label.Color}");
                }

                var cardDto = new TrelloCardDto
                {
                    Id = card.Id,
                    Name = card.Name,
                    Description = card.Description ?? string.Empty,
                    Labels = card.Labels?.Select(l => l.Name ?? "").Where(n => !string.IsNullOrEmpty(n)).ToList() ?? new List<string>(),
                    LastModified = card.LastActivity ?? DateTime.MinValue
                };

                cardDtos.Add(cardDto);
            }

            return cardDtos;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving Todo cards: {ex.Message}", ex);
        }
    }

    public async Task PostCommentAsync(string cardId, string comment)
    {
        TrelloAuthorization.Default.AppKey = applicationSettings.TrelloApiKey;
        TrelloAuthorization.Default.UserToken = applicationSettings.TrelloApiToken;

        try
        {
            var card = new Card(cardId);
            await card.Refresh();

            await card.Comments.Add(comment);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error posting comment to card {cardId}: {ex.Message}", ex);
        }
    }

    public async Task<bool> HasAIInvestigationCommentAsync(string cardId)
    {
        TrelloAuthorization.Default.AppKey = applicationSettings.TrelloApiKey;
        TrelloAuthorization.Default.UserToken = applicationSettings.TrelloApiToken;

        try
        {
            var card = new Card(cardId);
            await card.Refresh();
            await card.Comments.Refresh();

            // Check if any comment starts with the AI investigation sentence
            return card.Comments.Any(comment =>
                comment.Data?.Text?.StartsWith("This investigation was automated with the use of AI") == true);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error checking comments for card {cardId}: {ex.Message}", ex);
        }
    }

    public async Task<bool> HasProductivityCommentAsync(string cardId)
    {
        TrelloAuthorization.Default.AppKey = applicationSettings.TrelloApiKey;
        TrelloAuthorization.Default.UserToken = applicationSettings.TrelloApiToken;

        try
        {
            var card = new Card(cardId);
            await card.Refresh();
            await card.Comments.Refresh();

            // Check if any comment starts with the productivity agent sentence
            return card.Comments.Any(comment =>
                comment.Data?.Text?.StartsWith("Productivity agent created this pull request:") == true);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error checking comments for card {cardId}: {ex.Message}", ex);
        }
    }

}