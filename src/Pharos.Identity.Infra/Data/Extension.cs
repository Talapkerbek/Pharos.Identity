using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Pharos.Identity.Infra.Data;

public static class Extension
{
    public static IServiceCollection AddEFDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgresSQL");

        if (connectionString == null)
        {
            throw new NullReferenceException();
        }
        
        services.AddDbContext<ApplicationDbContext>
        (
            options =>
            {
                options.UseNpgsql(connectionString);
                options.UseOpenIddict();
            },
            optionsLifetime: ServiceLifetime.Singleton
        );
        
        return services;
    }
}