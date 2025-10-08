using Application.Model;
using System.Text.Json;

namespace Application.Helpers;

public static class TopicsExtension
{
    public static InvestigationTopics ParseTopicExtraction(string jsonResponse)
    {
        if (string.IsNullOrWhiteSpace(jsonResponse))
        {
            return new InvestigationTopics();
        }

        try
        {
            // Clean the response in case there's extra text
            var cleanJson = jsonResponse.Trim();
            if (cleanJson.StartsWith("```json"))
            {
                cleanJson = cleanJson.Substring(7);
            }
            if (cleanJson.EndsWith("```"))
            {
                cleanJson = cleanJson.Substring(0, cleanJson.Length - 3);
            }
            cleanJson = cleanJson.Trim();

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var parsed = JsonSerializer.Deserialize<InvestigationTopics>(cleanJson, jsonOptions);
            return parsed ?? new InvestigationTopics();
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse topic extraction JSON: {ex.Message}. Response was: {jsonResponse}");
        }
    }

    public static CodeChangeTopics ParseCodeChangeTopics(string jsonResponse)
    {
        if (string.IsNullOrWhiteSpace(jsonResponse))
        {
            return new CodeChangeTopics();
        }

        try
        {
            // Clean the response in case there's extra text
            var cleanJson = jsonResponse.Trim();
            if (cleanJson.StartsWith("```json"))
            {
                cleanJson = cleanJson.Substring(7);
            }
            if (cleanJson.EndsWith("```"))
            {
                cleanJson = cleanJson.Substring(0, cleanJson.Length - 3);
            }
            cleanJson = cleanJson.Trim();

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var parsed = JsonSerializer.Deserialize<CodeChangeTopics>(cleanJson, jsonOptions);
            return parsed ?? new CodeChangeTopics();
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse code change topics JSON: {ex.Message}. Response was: {jsonResponse}");
        }
    }

    public static FileChange[] ParseCodeChangeSuggestions(string jsonResponse)
    {
        if (string.IsNullOrWhiteSpace(jsonResponse))
        {
            return [];
        }

        try
        {
            // Clean the response in case there's extra text
            var cleanJson = jsonResponse.Trim();
            if (cleanJson.StartsWith("```json"))
            {
                cleanJson = cleanJson.Substring(7);
            }
            if (cleanJson.EndsWith("```"))
            {
                cleanJson = cleanJson.Substring(0, cleanJson.Length - 3);
            }
            cleanJson = cleanJson.Trim();

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var parsed = JsonSerializer.Deserialize<FileChange[]>(cleanJson, jsonOptions);
            return parsed ?? [];
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse code change suggestions JSON: {ex.Message}. Response was: {jsonResponse}");
        }
    }
}