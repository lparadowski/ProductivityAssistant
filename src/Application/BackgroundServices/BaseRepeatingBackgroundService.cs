using Application.BackgroundServices.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Application.BackgroundServices;

public abstract class BaseRepeatingBackgroundService<TRepeatingBackgroundService> : BackgroundService where TRepeatingBackgroundService : IAsyncRepeatingBackgroundService
{
    private int _repetitionIntervalInSeconds { get; set; }
    private int _currentRetryDelayInSeconds;

    protected BaseRepeatingBackgroundService(int repetitionIntervalInSeconds = 10, int maxRetryDelayInSeconds = 300)
    {
        _repetitionIntervalInSeconds = repetitionIntervalInSeconds;
        _currentRetryDelayInSeconds = repetitionIntervalInSeconds;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await DoWork(cancellationToken).ConfigureAwait(false);
    }

    //Todo : Polly policy for retries with exponential backoff
    public async Task DoWork(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await RepeatedWorkAsync(cancellationToken).ConfigureAwait(false);

                await Task.Delay(TimeSpan.FromSeconds(_repetitionIntervalInSeconds), cancellationToken).ConfigureAwait(false);

                _currentRetryDelayInSeconds = _repetitionIntervalInSeconds;
            }
            catch
            {
                await Task.Delay(TimeSpan.FromSeconds(_currentRetryDelayInSeconds), cancellationToken).ConfigureAwait(false);

                // We don't throw the exception here as we want the service to recover if possible. Should log it though.
            }
        }
    }

    public abstract Task RepeatedWorkAsync(CancellationToken cancellationToken);

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask.ConfigureAwait(false);
    }
}