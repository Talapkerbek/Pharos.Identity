using System.Text.Json;
using Pharos.Identity.Infra.Data;
using StackExchange.Redis;

namespace Pharos.Identity.Infra.Redis.CacheServices;


public interface IUserCacheService
{
    Task<UserDto?> GetUserAsync(string userId);
    Task SetUserAsync(UserDto user, TimeSpan? expiry = null);
    Task InvalidateUserAsync(string userId);
}

public class UserCacheService : IUserCacheService
{
    private readonly IDatabase _db;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
    private const string Prefix = "user:";

    public UserCacheService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<UserDto?> GetUserAsync(string userId)
    {
        var data = await _db.StringGetAsync(Prefix + userId);
        if (data.IsNullOrEmpty) return null;

        return JsonSerializer.Deserialize<UserDto>(data!, _jsonOptions);
    }

    public async Task SetUserAsync(UserDto user, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(user, _jsonOptions);
        await _db.StringSetAsync(Prefix + user.Id, json, expiry ?? TimeSpan.FromHours(1));
    }

    public async Task InvalidateUserAsync(string userId)
    {
        await _db.KeyDeleteAsync(Prefix + userId);
    }
}