using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using Application.Helpers;
using Application.Interfaces;
using Application.Model;

namespace Application.Services;

public class ClaudeApiService(AnthropicClient anthropicClient) : IClaudeApiService
{
    //Could make this configurable if Haiku35 can handle it.
    private string _anthropicModel => AnthropicModels.Claude45Sonnet;

    public async Task<InvestigationTopics> ExtractTopicsAsync(string cardTitle, string cardDescription)
    {
        var prompt = PromptHelper.BuildTopicExtractionPrompt(cardTitle, cardDescription);

        var messages = new List<Message>
        {
            new Message(RoleType.User, prompt)
        };

        var parameters = new MessageParameters
        {
            Messages = messages,
            Model = _anthropicModel,
            MaxTokens = 1000,
            Temperature = 0.1m // Low temperature for consistent, structured output
        };

        try
        {
            var response = await anthropicClient.Messages.GetClaudeMessageAsync(parameters);
            var content = string.Empty;

            if (response.Content.FirstOrDefault() is TextContent textContent)
            {
                content = textContent.Text;
            }

            // Parse the JSON response
            return TopicsExtension.ParseTopicExtraction(content);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error extracting topics: {ex.Message}", ex);
        }
    }

    public async Task<string> InvestigateWithContextAsync(string cardTitle, string cardDescription, CodeSample[] codeSamples)
    {
        var prompt = PromptHelper.BuildInvestigationPrompt(cardTitle, cardDescription, codeSamples);

        // Estimate tokens (rough approximation: 4 chars = 1 token)
        var estimatedTokens = prompt.Length / 4;
        if (estimatedTokens > 200_000)
        {
            Console.WriteLine($"Warning: Investigation prompt is large ({estimatedTokens:N0} estimated tokens, >200K threshold)");
        }

        var messages = new List<Message>
        {
            new Message(RoleType.User, prompt)
        };

        var parameters = new MessageParameters
        {
            Messages = messages,
            Model = _anthropicModel,
            MaxTokens = 8192,
            Temperature = 0.3m // Slightly higher temperature for more creative investigation
        };

        try
        {
            var response = await anthropicClient.Messages.GetClaudeMessageAsync(parameters);

            // Log token usage and cost
            LogTokenUsage("Investigation", response);

            if (response.Content.FirstOrDefault() is TextContent textContent)
            {
                return textContent.Text;
            }

            return "No response received from Claude API.";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error performing investigation: {ex.Message}", ex);
        }
    }

    public async Task<CodeChangeTopics> ExtractCodeChangeTopicsAsync(string cardTitle, string cardDescription)
    {
        var prompt = PromptHelper.BuildCodeChangeTopicExtractionPrompt(cardTitle, cardDescription);

        var messages = new List<Message>
        {
            new Message(RoleType.User, prompt)
        };

        var parameters = new MessageParameters
        {
            Messages = messages,
            Model = _anthropicModel,
            MaxTokens = 1000,
            Temperature = 0.1m // Low temperature for consistent, structured output
        };

        try
        {
            var response = await anthropicClient.Messages.GetClaudeMessageAsync(parameters);
            var content = string.Empty;

            if (response.Content.FirstOrDefault() is TextContent textContent)
            {
                content = textContent.Text;
            }

            // Parse the JSON response
            return TopicsExtension.ParseCodeChangeTopics(content);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error extracting code change topics: {ex.Message}", ex);
        }
    }

    private void LogTokenUsage(string operation, MessageResponse response)
    {
        if (response.Usage != null)
        {
            var inputTokens = response.Usage.InputTokens;
            var outputTokens = response.Usage.OutputTokens;
            var totalTokens = inputTokens + outputTokens;

            // Claude 4 Sonnet pricing: ≤200K: $3 input/$15 output, >200K: $6 input/$22.50 output
            var inputRate = inputTokens <= 200_000 ? 3.0 : 6.0;
            var outputRate = inputTokens <= 200_000 ? 15.0 : 22.50;

            var inputCost = (inputTokens / 1_000_000.0) * inputRate;
            var outputCost = (outputTokens / 1_000_000.0) * outputRate;
            var totalCost = inputCost + outputCost;

            Console.WriteLine($"{operation} - Tokens: {inputTokens:N0} input + {outputTokens:N0} output = {totalTokens:N0} total");
            Console.WriteLine($"{operation} - Cost: ${inputCost:F4} input + ${outputCost:F4} output = ${totalCost:F4} total");
        }
    }

    public async Task<FileChange[]> SuggestCodeChangesWithContextAsync(string cardTitle, string cardDescription, CodeSample[] codeSamples)
    {
        var prompt = PromptHelper.BuildCodeChangeSuggestionPrompt(cardTitle, cardDescription, codeSamples);

        // Estimate tokens (rough approximation: 4 chars = 1 token)
        var estimatedTokens = prompt.Length / 4;
        if (estimatedTokens > 200_000)
        {
            Console.WriteLine($"Warning: Code change suggestion prompt is large ({estimatedTokens:N0} estimated tokens, >200K threshold)");
        }

        var messages = new List<Message>
        {
            new Message(RoleType.User, prompt)
        };

        var parameters = new MessageParameters
        {
            Messages = messages,
            Model = _anthropicModel,
            MaxTokens = 8192,
            Temperature = 0.2m // Low temperature for consistent, precise code suggestions
        };

        try
        {
            var response = await anthropicClient.Messages.GetClaudeMessageAsync(parameters);

            // Log token usage and cost
            LogTokenUsage("Code Change Suggestion", response);

            if (response.Content.FirstOrDefault() is TextContent textContent)
            {
                return TopicsExtension.ParseCodeChangeSuggestions(textContent.Text);
            }

            return [];
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error generating code change suggestions: {ex.Message}", ex);
        }
    }
}