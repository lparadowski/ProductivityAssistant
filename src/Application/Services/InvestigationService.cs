using Application.Interfaces;

namespace Application.Services;

public class InvestigationService(IClaudeApiService claudeApiService, ICodeSampleLocator codeSampleLocator) : IInvestigationService
{
    public async Task<string> InvestigateAsync(string cardTitle, string cardDescription, string codebasePath)
    {
        //1. Get the filtered file list from the codebase.
        var fileList = await codeSampleLocator.GetFilteredFileListAsync(codebasePath);

        //2. Ask Claude to determine which files are relevant to the ticket.
        var relevantFiles = await claudeApiService.DetermineRelevantFilesAsync(cardTitle, cardDescription, fileList);

        //3. Retrieve the content of the relevant files.
        var codeSamples = await codeSampleLocator.GetCodeSamplesForFilesAsync(relevantFiles, codebasePath);

        //4. Send to Claude API for investigation.
        return await claudeApiService.InvestigateWithContextAsync(cardTitle, cardDescription, codeSamples);
    }
}
