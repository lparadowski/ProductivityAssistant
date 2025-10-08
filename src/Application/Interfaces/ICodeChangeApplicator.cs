using Application.Model;

namespace Application.Interfaces;

public interface ICodeChangeApplicator
{
    Task ApplyChanges(FileChange[] suggestions, string baseDirectory);
}