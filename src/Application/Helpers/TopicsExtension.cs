using Application.Model;
using System.Text.Json;

namespace Application.Helpers;

public static class TopicsExtension
{
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

            cleanJson = SanitizeJsonEscapes(cleanJson);

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

    private static string SanitizeJsonEscapes(string json)
    {
        // Fix invalid \u sequences (must be followed by exactly 4 hex digits)
        // Replace invalid escape sequences like \u followed by non-hex with escaped backslash
        var result = System.Text.RegularExpressions.Regex.Replace(
            json,
            @"\\u(?![0-9a-fA-F]{4})",
            @"\\u");

        // Fix other common invalid escapes (backslash followed by letters that aren't valid escapes)
        // Valid JSON escapes: \", \\, \/, \b, \f, \n, \r, \t, \uXXXX
        result = System.Text.RegularExpressions.Regex.Replace(
            result,
            @"\\([^""\\\/bfnrtu])",
            @"\\$1");

        return result;
    }
}
