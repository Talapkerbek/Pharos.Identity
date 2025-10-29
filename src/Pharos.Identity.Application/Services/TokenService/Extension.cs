using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace Pharos.Identity.Application.Services.TokenService;

public static class Extension
{
    public static IServiceCollection AddTokenService(this IServiceCollection services)
    {
        services.AddHttpClient<ITokenService, TokenService>(c =>
        {
            // config
        })
        .AddTransientHttpErrorPolicy(policy => 
            policy.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
        );;
        
        return services;
    }
}