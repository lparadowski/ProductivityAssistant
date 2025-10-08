using Application.Dtos;

namespace Application.Interfaces;

public interface ITrelloService
{
    Task<IEnumerable<TrelloCardDto>> GetTodoCardsAsync(string boardId);
    Task PostCommentAsync(string cardId, string comment);
    Task<bool> HasAIInvestigationCommentAsync(string cardId);
    Task<bool> HasProductivityCommentAsync(string cardId);
}