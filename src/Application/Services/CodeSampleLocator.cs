using Application.Interfaces;
using Application.Model;

namespace Application.Services;

public class CodeSampleLocator : ICodeSampleLocator
{
    private static readonly string[] BlacklistedFolders = [".vs", "bin", "obj", "node_modules", ".git", "packages", "TestResults"];

    public async Task<string[]> GetFilteredFileListAsync(string codebasePath)
    {
        return await Task.Run(() =>
        {
            if (string.IsNullOrEmpty(codebasePath) || !Directory.Exists(codebasePath))
            {
                throw new DirectoryNotFoundException($"Codebase directory not found: {codebasePath}");
            }

            return Directory.GetFiles(codebasePath, "*.cs", SearchOption.AllDirectories)
                .Where(f => !BlacklistedFolders.Any(folder =>
                    f.Contains($"{Path.DirectorySeparatorChar}{folder}{Path.DirectorySeparatorChar}")))
                .Select(f => Path.GetRelativePath(codebasePath, f))
                .OrderBy(f => f)
                .ToArray();
        });
    }

    public async Task<CodeSample[]> GetCodeSamplesForFilesAsync(string[] filePaths, string codebasePath)
    {
        var codeSamples = new List<CodeSample>();

        foreach (var relativePath in filePaths)
        {
            try
            {
                var fullPath = Path.Combine(codebasePath, relativePath);

                if (!File.Exists(fullPath))
                    continue;

                var content = await File.ReadAllTextAsync(fullPath);

                codeSamples.Add(new CodeSample
                {
                    FilePath = relativePath,
                    Section = content,
                    SectionType = "complete_file",
                    LineNumber = 1,
                    RelevanceScore = 10,
                    MatchReason = "Selected by Claude"
                });
            }
            catch (Exception)
            {
                // Skip files that can't be read
            }
        }

        return codeSamples.ToArray();
    }
}
