using Application.Interfaces;
using Application.Model;
using Application.Settings;
using System.Text.RegularExpressions;

namespace Application.Services;

public class CodeSampleLocator(ApplicationSettings settings) : ICodeSampleLocator
{
    public async Task<CodeSample[]> FindRelevantCodeAsync(InvestigationTopics topics, string codebasePath)
    {
        var codeSamples = new List<CodeSample>();

        var candidateFiles = await FindCandidateFilesAsync(topics.FilePatterns, codebasePath);

        foreach (var filePath in candidateFiles)
        {
            try
            {
                var samples = await ExtractRelevantSectionsAsync(filePath, topics.Technologies, codebasePath);
                codeSamples.AddRange(samples);
            }
            catch (Exception ex)
            {
                //TODO: Log but continue with other files.
            }
        }

        return codeSamples.OrderByDescending(s => s.RelevanceScore).ToArray();
    }

    public async Task<CodeSample[]> FindRelevantCodeAsync(CodeChangeTopics topics, string codebasePath)
    {
        var codeSamples = new List<CodeSample>();

        var candidateFiles = await FindCandidateFilesAsync(topics.FilePatterns, codebasePath);

        // If no files found for the target entity, look for sibling examples (e.g. if changing "Order" entity, look for OrderController, OrderService, etc.) If no "Order" files
        // exist yet, look for other domain entities (siblings) and their related files.
        if (candidateFiles.Length == 0)
        {
            candidateFiles = await FindSiblingExamples(topics, codebasePath);
        }

        foreach (var filePath in candidateFiles)
        {
            try
            {
                //For a code change, we want to send the complete file to understand context.
                var sample = await ExtractCompleteFileAsync(filePath, topics, codebasePath);
                if (sample != null)
                {
                    codeSamples.Add(sample);
                }
            }
            catch (Exception ex)
            {
                //TODO: Log but continue with other files.
            }
        }

        return codeSamples.OrderByDescending(s => s.RelevanceScore).ToArray();
    }

    private async Task<string[]> FindCandidateFilesAsync(string[] filePatterns, string codebasePath)
    {
        return await Task.Run(() =>
        {
            var files = new HashSet<string>();
            var baseDirectory = codebasePath;

            if (string.IsNullOrEmpty(baseDirectory) || !Directory.Exists(baseDirectory))
            {
                throw new DirectoryNotFoundException($"Codebase directory not found: {baseDirectory}");
            }

            var blacklistedFolders = new[] { ".vs", "bin", "obj", "node_modules", ".git", "packages", "TestResults" };

            foreach (var pattern in filePatterns)
            {
                try
                {
                    // Convert glob pattern to search pattern
                    var searchPattern = pattern.Replace("*", "");
                    var matchingFiles = Directory.GetFiles(baseDirectory, "*.cs", SearchOption.AllDirectories)
                        .Where(f => f.Contains(searchPattern, StringComparison.OrdinalIgnoreCase))
                        .Where(f => !blacklistedFolders.Any(folder => f.Contains($"{Path.DirectorySeparatorChar}{folder}{Path.DirectorySeparatorChar}")))
                        .ToArray();

                    foreach (var file in matchingFiles)
                    {
                        files.Add(file);
                    }

                    // Also try exact glob matching for more complex patterns
                    if (pattern.Contains("*"))
                    {
                        var regexPattern = "^" + Regex.Escape(pattern).Replace(@"\*", ".*") + "$";
                        var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);

                        var allFiles = Directory.GetFiles(baseDirectory, "*.cs", SearchOption.AllDirectories)
                            .Where(f => regex.IsMatch(Path.GetFileName(f)))
                            .Where(f => !blacklistedFolders.Any(folder => f.Contains($"{Path.DirectorySeparatorChar}{folder}{Path.DirectorySeparatorChar}")))
                            .ToArray();

                        foreach (var file in allFiles)
                        {
                            files.Add(file);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not search for pattern {pattern}: {ex.Message}");
                }
            }

            return files.ToArray();
        });
    }

    private async Task<string[]> FindSiblingExamples(CodeChangeTopics topics, string codebasePath)
    {
        return await Task.Run(() =>
        {
            var siblingFiles = new List<string>();
            var baseDirectory = codebasePath;
            var blacklistedFolders = new[] { ".vs", "bin", "obj", "node_modules", ".git", "packages", "TestResults" };

            var domainPath = Path.Combine(baseDirectory, "src", "Domain", "Entities");
            if (!Directory.Exists(domainPath))
            {
                return Array.Empty<string>();
            }

            //If you have a large number of domain entities, you might want to use "Take" to limit the number of siblings.
            var domainFiles = Directory.GetFiles(domainPath, "*.cs", SearchOption.TopDirectoryOnly)
                .Where(f => !f.Contains("Enum") && !f.Contains("Base"));

            foreach (var domainFile in domainFiles)
            {
                var entityName = Path.GetFileNameWithoutExtension(domainFile);

                // Add the domain entity itself
                siblingFiles.Add(domainFile);

                // Look for related files across layers
                var relatedPatterns = new[]
                {
                $"*{entityName}Controller*.cs",
                $"*{entityName}Service*.cs",
                $"*{entityName}Repository*.cs",
                $"*{entityName}Request*.cs",
                $"*{entityName}Response*.cs"
            };

                foreach (var pattern in relatedPatterns)
                {
                    var matchingFiles = Directory.GetFiles(baseDirectory, pattern, SearchOption.AllDirectories)
                        .Where(f => !blacklistedFolders.Any(folder => f.Contains($"{Path.DirectorySeparatorChar}{folder}{Path.DirectorySeparatorChar}")))
                        .ToArray();

                    siblingFiles.AddRange(matchingFiles);
                }
            }

            return siblingFiles.Distinct().ToArray();
        });
    }

    private async Task<CodeSample[]> ExtractRelevantSectionsAsync(string filePath, string[] technologies, string codebasePath)
    {
        var content = await File.ReadAllTextAsync(filePath);
        var lines = content.Split('\n');
        var samples = new List<CodeSample>();

        // Simple approach: find classes and methods that contain technology keywords
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            // Check if line contains any technology keywords
            var matchedTechs = technologies.Where(tech =>
                line.Contains(tech, StringComparison.OrdinalIgnoreCase)).ToArray();

            if (matchedTechs.Any())
            {
                // Try to extract the containing method or class
                var section = ExtractContainingSection(lines, i, out var sectionType, out var startLine);

                if (!string.IsNullOrEmpty(section))
                {
                    var relevanceScore = CalculateRelevanceScore(section, matchedTechs, Path.GetFileName(filePath));

                    samples.Add(new CodeSample
                    {
                        FilePath = Path.GetRelativePath(codebasePath, filePath),
                        Section = section,
                        SectionType = sectionType,
                        LineNumber = startLine + 1, // 1-based line numbers
                        RelevanceScore = relevanceScore,
                        MatchReason = $"Contains technologies: {string.Join(", ", matchedTechs)}"
                    });
                }
            }
        }

        return samples.ToArray();
    }

    private async Task<CodeSample?> ExtractCompleteFileAsync(string filePath, CodeChangeTopics topics, string codebasePath)
    {
        var content = await File.ReadAllTextAsync(filePath);
        var searchKeywords = topics.Entities.Concat(topics.Operations).Concat(topics.Layers).ToArray();

        // Check if file is relevant by searching for keywords
        var isRelevant = searchKeywords.Any(keyword =>
            content.Contains(keyword, StringComparison.OrdinalIgnoreCase));

        if (!isRelevant)
        {
            return null;
        }

        // Calculate relevance score based on keyword matches
        var matchedKeywords = searchKeywords.Where(keyword =>
            content.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToArray();

        var relevanceScore = CalculateRelevanceScore(content, matchedKeywords, Path.GetFileName(filePath));

        return new CodeSample
        {
            FilePath = Path.GetRelativePath(codebasePath, filePath),
            Section = content, // Return complete file content
            SectionType = "complete_file",
            LineNumber = 1,
            RelevanceScore = relevanceScore,
            MatchReason = $"Complete file containing: {string.Join(", ", matchedKeywords)}"
        };
    }

    private string ExtractContainingSection(string[] lines, int matchLineIndex, out string sectionType, out int startLine)
    {
        sectionType = "unknown";
        startLine = matchLineIndex;

        // Skip if the match is just in using statements at the top
        if (matchLineIndex < 10 && lines[matchLineIndex].Trim().StartsWith("using"))
        {
            // Look for the first actual class/method after the using statements
            for (int i = matchLineIndex + 1; i < Math.Min(lines.Length, matchLineIndex + 50); i++)
            {
                var line = lines[i].Trim();
                if (line.Contains("class ") && (line.Contains("public") || line.Contains("internal") || line.Contains("private")))
                {
                    sectionType = "class";
                    startLine = i;
                    return ExtractBlock(lines, i);
                }
            }
        }

        // Look backwards to find containing structure
        for (int i = matchLineIndex; i >= 0; i--)
        {
            var line = lines[i].Trim();

            // Check for class declaration
            if (line.Contains("class ") && (line.Contains("public") || line.Contains("internal") || line.Contains("private")))
            {
                sectionType = "class";
                startLine = i;
                return ExtractBlock(lines, i);
            }

            // Check for method declaration
            if (IsMethodDeclaration(line))
            {
                sectionType = "method";
                startLine = i;
                return ExtractBlock(lines, i);
            }

            // Check for constructor
            if (line.Contains("public ") && line.Contains("(") && !line.Contains("class") && !line.Contains("void") && !line.Contains("return"))
            {
                sectionType = "constructor";
                startLine = i;
                return ExtractBlock(lines, i);
            }

            // Check for property
            if (line.Contains("get;") || line.Contains("set;") || line.Contains("=>"))
            {
                sectionType = "property";
                startLine = i;
                // For properties, return a few lines of context
                var start = Math.Max(0, i - 2);
                var end = Math.Min(lines.Length - 1, i + 2);
                return string.Join("\n", lines[start..(end + 1)]);
            }
        }

        // Enhanced context extraction - look for meaningful blocks
        sectionType = "context";
        var contextStart = Math.Max(0, matchLineIndex - 5);
        var contextEnd = Math.Min(lines.Length - 1, matchLineIndex + 10);
        startLine = contextStart;
        return string.Join("\n", lines[contextStart..(contextEnd + 1)]);
    }

    private bool IsMethodDeclaration(string line)
    {
        return (line.Contains("public ") || line.Contains("private ") || line.Contains("protected ") || line.Contains("internal ")) &&
               line.Contains("(") && !line.Contains("class ") && !line.TrimStart().StartsWith("//");
    }

    private string ExtractBlock(string[] lines, int startIndex)
    {
        var result = new List<string>();
        var braceCount = 0;
        bool foundFirstBrace = false;

        for (int i = startIndex; i < lines.Length; i++)
        {
            var line = lines[i];
            result.Add(line);

            // Count braces to find the end of the block
            for (int j = 0; j < line.Length; j++)
            {
                if (line[j] == '{')
                {
                    braceCount++;
                    foundFirstBrace = true;
                }
                else if (line[j] == '}')
                {
                    braceCount--;
                }
            }

            // If we've found the opening brace and closed all braces, we're done
            if (foundFirstBrace && braceCount == 0)
            {
                break;
            }

            // Safety: don't extract more than 100 lines for a single block
            if (result.Count > 100)
            {
                result.Add("// ... (truncated for length)");
                break;
            }
        }

        return string.Join("\n", result);
    }

    private int CalculateRelevanceScore(string section, string[] matchedTechnologies, string fileName)
    {
        int score = 5;

        // Higher score for more technology matches
        score += matchedTechnologies.Length * 2;

        // Higher score for files with relevant names
        foreach (var tech in matchedTechnologies)
        {
            if (fileName.Contains(tech, StringComparison.OrdinalIgnoreCase))
            {
                score += 3;
            }
        }

        // Higher score for class declarations vs context
        if (section.Contains("public class"))
        {
            score += 2;
        }

        // Cap at 10
        return Math.Min(10, score);
    }
}