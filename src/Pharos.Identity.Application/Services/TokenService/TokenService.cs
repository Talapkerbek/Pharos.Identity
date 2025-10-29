using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Pharos.Identity.Application.Services.TokenService;

public class TokenService : ITokenService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;

    private const string CacheKey = "ClientAccessToken";

    public TokenService(HttpClient httpClient, IConfiguration configuration, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _cache = cache;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue<string>(CacheKey, out var token))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(token);
            Console.ResetColor();
            return token ?? "";
        }

        var authority = _configuration["ServiceSettings:Authority"];
        var clientId = _configuration["ServiceSettings:ClientId"];
        var clientSecret = _configuration["ServiceSettings:ClientSecret"];

        var tokenRequest = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = clientId!,
            ["client_secret"] = clientSecret!,
            ["scope"] = "Media.fullaccess",
        };

        var response = await _httpClient.PostAsync($"{authority}/connect/token", new FormUrlEncodedContent(tokenRequest), ct);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        var accessToken = payload.GetProperty("access_token").GetString()!;
        
        var expiresIn = payload.GetProperty("expires_in").GetInt32();
        var expiration = TimeSpan.FromSeconds(expiresIn - 10);
        _cache.Set(CacheKey, accessToken, expiration);

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(payload.ToString());
        Console.ResetColor();
        
        return payload.GetProperty("access_token").GetString()!;
    }
}