using Application.Interfaces;
using Infrastructure.ApiRequests;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IGithubApiService, GithubApiService>();

        return services;
    }
}