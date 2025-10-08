using Application.Model;

namespace Application.Helpers;

public static class PromptHelper
{
    public static string BuildTopicExtractionPrompt(string cardTitle, string cardDescription)
    {
        return $@"Analyze this investigation ticket and extract key information.

Title: {cardTitle}
Description: {cardDescription}

Extract:
1. Technologies mentioned or implied (libraries, frameworks, tools)
2. Actions to be performed (investigate, replace, implement, analyze, etc.)
3. File patterns to search for relevant code (use wildcards like *.cs, *Mapping*, *Cache*, etc.)

Return ONLY valid JSON in this exact format:
{{
  ""technologies"": [""AutoMapper"", ""Redis""],
  ""actions"": [""investigate"", ""replace""],
  ""filePatterns"": [""*Mapping*.cs"", ""*Cache*.cs"", ""*.config""]
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
8. Keep your response under 8000 tokens to ensure complete delivery

Please conduct a thorough investigation of this ticket using the provided code context.";

        return prompt;
    }

    public static string BuildCodeChangeTopicExtractionPrompt(string cardTitle, string cardDescription)
    {
        return $@"Analyze this code change ticket and extract key information for implementing the change.

Title: {cardTitle}
Description: {cardDescription}

Extract:
1. Domain entities (objects/models like Invoice, Customer, Product, etc.)
2. Operations (CRUD actions: Create, Read, Update, Delete, Add, Remove, etc.)
3. Architectural layers needed (API, Service, Repository, Domain, etc.)
4. Specific file patterns for the entities (use entity names: *Invoice*.cs, *Customer*.cs)
5. Configuration/infrastructure patterns if the ticket mentions:
   - DI registration, dependency injection, service registration → include ""*ServiceCollectionExtensions*.cs"", ""Startup.cs"", ""Program.cs""
   - Configuration, settings, appsettings → include ""*Settings*.cs"", ""appsettings*.json""
   - Database migrations, schema → include ""*Migration*.cs"", ""*DbContext*.cs""

Return ONLY valid JSON in this exact format:
{{
  ""entities"": [""Invoice"", ""Customer""],
  ""operations"": [""delete"", ""create""],
  ""layers"": [""API"", ""Service"", ""Repository"", ""Domain""],
  ""filePatterns"": [""*Invoice*.cs"", ""*Customer*.cs"", ""*ServiceCollectionExtensions*.cs""]
}}

IMPORTANT:
- Include infrastructure patterns in filePatterns when the ticket explicitly mentions DI, configuration, or database changes
- Focus on specific entities and targeted file patterns, not generic patterns
Do not include any text before or after the JSON.";
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