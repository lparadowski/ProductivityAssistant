namespace Application.Interfaces;

public interface IInvestigationService
{
    Task<string> InvestigateAsync(string cardTitle, string cardDescription, string codebasePath);
}