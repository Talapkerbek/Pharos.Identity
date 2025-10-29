namespace Pharos.Identity.Application.Services.TokenService;

public interface ITokenService
{
    Task<string> GetAccessTokenAsync(CancellationToken ct = default);
}