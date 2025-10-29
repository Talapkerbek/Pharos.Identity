using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace Pharos.Identity.Application.Services.MediaClient;


public static class Extension
{
    public static IServiceCollection AddMediaServiceClient(this IServiceCollection services)
    {
        services.AddHttpClient<IMediaServiceClient, MediaServiceClient>(c =>
            {
                // config
            })
            .AddTransientHttpErrorPolicy(policy => 
                policy.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
            );;
        
        return services;
    }
}