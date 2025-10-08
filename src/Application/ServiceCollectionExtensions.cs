using Application.BackgroundServices;
using Application.Interfaces;
using Application.Services;
using Application.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, ApplicationSettings applicationSettings)
    {
        services.AddSingleton(applicationSettings);
        services.AddHostedService<TrelloBackgroundService>();
        services.AddScoped<ITrelloService, TrelloService>();
        services.AddScoped<IBoardProcessor, BoardProcessor>();
        services.AddScoped<IInvestigationService, InvestigationService>();
        services.AddScoped<IClaudeApiService, ClaudeApiService>();
        services.AddScoped<ICodeSampleLocator, CodeSampleLocator>();
        services.AddScoped<ICodeChangeApplicator, CodeChangeApplicator>();
        services.AddScoped<IGithubService, GithubService>();
        services.AddScoped<ICodingService, CodingService>();
        services.Configure<HostOptions>(hostOptions =>
        {
            // We want the service to continue and try to recover itself if it encounters an exception.
            hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
        });

        return services;
    }
}