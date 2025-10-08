namespace Application.Dtos;

public class TrelloCardDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Labels { get; set; } = new();
    public DateTime LastModified { get; set; } 
}