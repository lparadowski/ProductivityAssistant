using Application.BackgroundServices.Interfaces;
using Application.Interfaces;
using Application.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Application.BackgroundServices;

public class TrelloBackgroundService(IServiceProvider serviceProvider, ApplicationSettings applicationSettings) : BaseRepeatingBackgroundService<TrelloBackgroundService>(
    applicationSettings.ExecutionIntervalInSeconds,
    applicationSettings.MaxRetryDelayInSeconds), IAsyncRepeatingBackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private IBoardProcessor? _boardProcessor;

    public override async Task RepeatedWorkAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        _boardProcessor = scope.ServiceProvider.GetRequiredService<IBoardProcessor>();

        var boardIds = applicationSettings.TrelloBoardIds;

        foreach (var boardId in boardIds)
        {
            await _boardProcessor.ProcessBoardAsync(boardId);
        }
    }
}