using Application.Model;

namespace Application.Interfaces;

public interface ICodeSampleLocator
{
    Task<string[]> GetFilteredFileListAsync(string codebasePath);

    Task<CodeSample[]> GetCodeSamplesForFilesAsync(string[] filePaths, string codebasePath);
}
