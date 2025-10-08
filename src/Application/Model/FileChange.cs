namespace Application.Model;

public class FileChange
{
    public required string FilePath { get; set; }
    public required string Content { get; set; }
    public FileChangeType ChangeType { get; set; }
}

public enum FileChangeType
{
    Create,
    Modify,
    Delete
}