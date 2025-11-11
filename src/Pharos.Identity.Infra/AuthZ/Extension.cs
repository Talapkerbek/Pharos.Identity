using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pharos.FGA.AspNetCore.Authorization;
using Pharos.FGA.DependencyInjection;

namespace Pharos.Identity.Infra.AuthZ;

public static class Extension
{
    public static IServiceCollection ConfigureFgaAuthZ(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenFgaClient(conf =>
        {
            conf.ConfigureOpenFga(c => c.SetConnection(configuration["OpenFGA:ConnectionUrl"] ?? string.Empty));
            conf.SetStoreId(configuration["OpenFGA:StoreId"] ?? string.Empty);
        });

        services.AddOpenFgaMiddleware(conf =>
        {
            conf.SetUserIdentityResolver("user", principal => principal.Identity!.Name!);
        });

        services.AddAuthorizationBuilder()
            .AddPolicy(FgaAuthorizationDefaults.PolicyKey, p => p
                .RequireAuthenticatedUser()
                .AddFgaRequirement()
            );
        
        return services;
    }
}