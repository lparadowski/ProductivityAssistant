namespace Application.Interfaces;

public interface IBoardProcessor
{
    Task ProcessBoardAsync(string boardId);
}
