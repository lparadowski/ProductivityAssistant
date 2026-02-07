using Application.Model;

namespace Application.Helpers;

public static class PromptHelper
{
    public static string BuildFileSelectionPrompt(string cardTitle, string cardDescription, string[] fileList)
    {
        var fileListText = string.Join("\n", fileList);

        return $@"You are a senior software engineer. Given a ticket and a list of files in a .NET codebase, select the files most relevant to understanding or implementing this ticket.

TICKET:
Title: {cardTitle}
Description: {cardDescription}

FILE LIST:
{fileListText}

Select up to 20 files that are most relevant to this ticket. Consider:
- Files whose names match entities, services, or concepts mentioned in the ticket
- Files across architectural layers (Domain, Application, API, Infrastructure) that relate to the ticket
- Configuration or DI registration files if the ticket involves setup or wiring

Return ONLY valid JSON in this exact format:
{{
  ""selectedFiles"": [""src/Domain/Invoice.cs"", ""src/Application/Services/InvoiceService.cs""]
}}

Do not include any text before or after the JSON.";
    }

    public static string BuildInvestigationPrompt(string cardTitle, string cardDescription, CodeSample[] codeSamples)
    {
        var prompt = $@"You are a senior software architect conducting a technical investigation.

INVESTIGATION TICKET:
Title: {cardTitle}
Description: {cardDescription}

RELEVANT CODE CONTEXT:
I have analyzed the codebase and found {codeSamples.Length} relevant code sections:

";

        for (int i = 0; i < codeSamples.Length; i++)
        {
            var sample = codeSamples[i];
            prompt += $@"
CODE SAMPLE {i + 1}: {sample.FilePath}:{sample.LineNumber} ({sample.SectionType})
Match Reason: {sample.MatchReason}
Relevance Score: {sample.RelevanceScore}/10

```csharp
{sample.Section}
```

";
        }

        prompt += @"
INVESTIGATION REQUIREMENTS:
1. Start your response with exactly this sentence: ""This investigation was automated with the use of AI.""
2. Format your response for Trello using markdown (use ```csharp for code blocks, ## for headers, etc.)
3. Analyze the provided code context within a .NET Clean Architecture application
4. Provide specific, actionable findings based on the actual code shown
5. Include recommendations when applicable
6. Consider licensing, cost implications (time/resources), and technical debt
7. Structure your response with clear sections: Findings, Recommendations, Risk Assessment
8. Keep your response under 16000 characters (hard Trello limit) to ensure complete delivery. Aim for concise, actionable content

Please conduct a thorough investigation of this ticket using the provided code context.";

        return prompt;
    }

    public static string BuildCodeChangeSuggestionPrompt(string cardTitle, string cardDescription, CodeSample[] codeSamples)
    {
        var prompt = $@"You are a senior software engineer implementing code changes in a .NET Clean Architecture application.

CODE CHANGE TICKET:
Title: {cardTitle}
Description: {cardDescription}

RELEVANT CODE CONTEXT:
I have analyzed the codebase and found {codeSamples.Length} relevant code sections:

";

        for (int i = 0; i < codeSamples.Length; i++)
        {
            var sample = codeSamples[i];
            prompt += $@"
CODE SAMPLE {i + 1}: {sample.FilePath}:{sample.LineNumber} ({sample.SectionType})
Match Reason: {sample.MatchReason}
Relevance Score: {sample.RelevanceScore}/10

```csharp
{sample.Section}
```

";
        }

        prompt += @"
IMPLEMENTATION REQUIREMENTS:
1. Analyze the existing code patterns and architecture shown above
2. Suggest specific code changes needed to implement the requested feature
3. Follow the existing patterns, naming conventions, and architectural layers
4. Do not add any comments in the code examples - provide clean code without annotations
5. PRESERVE EXISTING: Keep ALL using statements exactly as they appear in the provided code samples - do not add, remove, or modify any using statements
6. DO NOT CHANGE: Namespace names, escape sequences in strings (like \\n in string literals), existing class names, or import paths
7. COPY EXACTLY: When modifying an existing file, copy the using statements from the provided code sample verbatim

Return ONLY valid JSON in this exact format:
[
  {
    ""filePath"": ""src/Domain/Invoice.cs"",
    ""content"": ""using MongoDB.Bson;\\nusing MongoDB.Bson.Serialization.Attributes;\\n\\nnamespace Domain;\\n\\npublic class Invoice\\n{\\n    // complete file content here\\n}"",
    ""changeType"": 1
  },
  {
    ""filePath"": ""src/Application/NewFile.cs"",
    ""content"": ""namespace Application;\\n\\npublic class NewFile\\n{\\n    // complete file content here\\n}"",
    ""changeType"": 0
  }
]

IMPORTANT:
- Use changeType: 1 for existing files that need modification
- Use changeType: 0 for new files that need to be created
- Provide complete file content, not just snippets
- Use proper escape sequences for newlines (\\n) and quotes (\\"")
- Use forward slashes in file paths (src/Domain/Invoice.cs)
- Do not include any text before or after the JSON";

        return prompt;
    }
}
