using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pharos.Identity.Infra.Redis.CacheServices;
using StackExchange.Redis;

namespace Pharos.Identity.Infra.Redis;

public static class RedisExtensions
{
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis")
                               ?? throw new InvalidOperationException("Redis connection string not configured.");

        var multiplexer = ConnectionMultiplexer.Connect(connectionString);

        services.AddSingleton<IConnectionMultiplexer>(multiplexer);
        services.AddSingleton<IUserCacheService, UserCacheService>();

        return services;
    }
}