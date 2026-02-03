using Application.Interfaces;

namespace Application.Services;

public class InvestigationService(IClaudeApiService claudeApiService, ICodeSampleLocator codeSampleLocator) : IInvestigationService
{
    public async Task<string> InvestigateAsync(string cardTitle, string cardDescription, string codebasePath)
    {
        //1. Analyse the card title and description to identify key topics and themes.
        var topics = await claudeApiService.ExtractCodeChangeTopicsAsync(cardTitle, cardDescription);

        //2. Search the repository for relevant files, code snippets, or documentation related to the identified topics.
        var codeSamples = await codeSampleLocator.FindRelevantCodeAsync(topics, codebasePath);

        //3. Create a prompt for Claude that includes the code samples to investigate the ticket based on the card title and description.
        //4. Send to Claude API for investigation.
        return await claudeApiService.InvestigateWithContextAsync(cardTitle, cardDescription, codeSamples);
    }
}