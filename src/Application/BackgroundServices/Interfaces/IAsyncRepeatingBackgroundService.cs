namespace Application.BackgroundServices.Interfaces;

public interface IAsyncRepeatingBackgroundService
{
    Task RepeatedWorkAsync(CancellationToken cancellationToken);
}