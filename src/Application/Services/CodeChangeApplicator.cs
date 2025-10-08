using Application.Interfaces;
using Application.Model;
using System.Text.Json;

namespace Application.Services;

public class CodeChangeApplicator : ICodeChangeApplicator
{
    public async Task ApplyChanges(FileChange[] suggestions, string baseDirectory)
    {
        foreach (var change in suggestions)
        {
            // Convert relative path to absolute path
            var absolutePath = Path.Combine(baseDirectory, change.FilePath.Replace('/', Path.DirectorySeparatorChar));

            var fileChange = new FileChange
            {
                FilePath = absolutePath,
                Content = change.Content,
                ChangeType = change.ChangeType // Direct enum assignment from JSON
            };

            await ApplyFileChange(fileChange);
        }
    }

    private async Task ApplyFileChange(FileChange change)
    {
        try
        {
            switch (change.ChangeType)
            {
                case FileChangeType.Create:
                    await CreateNewFile(change);
                    break;

                case FileChangeType.Modify:
                    await ModifyExistingFile(change);
                    break;

                case FileChangeType.Delete:
                    DeleteFile(change);
                    break;
            }

            Console.WriteLine($"Applied {change.ChangeType.ToString().ToLower()} to: {change.FilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying change to {change.FilePath}: {ex.Message}");
            throw;
        }
    }

    private async Task CreateNewFile(FileChange change)
    {
        // Ensure directory exists
        var directory = Path.GetDirectoryName(change.FilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Unescape content (convert \\n to actual newlines, \\t to tabs, etc.)
        var unescapedContent = UnescapeContent(change.Content);
        await File.WriteAllTextAsync(change.FilePath, unescapedContent);
    }

    private async Task ModifyExistingFile(FileChange change)
    {
        // Unescape content (convert \\n to actual newlines, \\t to tabs, etc.)
        var unescapedContent = UnescapeContent(change.Content);
        await File.WriteAllTextAsync(change.FilePath, unescapedContent);
    }

    private void DeleteFile(FileChange change)
    {
        if (File.Exists(change.FilePath))
        {
            File.Delete(change.FilePath);
        }
        else
        {
            Console.WriteLine($"Warning: File {change.FilePath} does not exist, skipping deletion");
        }
    }

    private static string UnescapeContent(string content)
    {
        // Only unescape if the content appears to be JSON-escaped (contains \\n but not proper newlines)
        if (content.Contains("\\n") && !content.Contains("\n"))
        {
            return content
                .Replace("\\n", "\n")
                .Replace("\\t", "\t")
                .Replace("\\r", "\r")
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\");
        }

        return content;
    }
}