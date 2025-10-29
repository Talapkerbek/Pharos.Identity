using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Pharos.Identity.Application.Services.TokenService;
using Pharos.Media.Contracts;

namespace Pharos.Identity.Application.Services.MediaClient;

public class MediaServiceClient : IMediaServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _config;

    public MediaServiceClient(HttpClient httpClient, ITokenService tokenService, IConfiguration config)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
        _config = config;
    }

    private async Task AddAuthHeaderAsync(CancellationToken ct)
    {
        var token = await _tokenService.GetAccessTokenAsync(ct);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<GetPresignedUrlResponse?> GetPresignedUrlAsync(GetPresignedUrlRequest request, CancellationToken ct)
    {
        await AddAuthHeaderAsync(ct);

        var response = await _httpClient.PostAsJsonAsync(
            $"{_config["MediaServiceUrl"]}/file/presigned_url",
            request,
            ct
        );

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GetPresignedUrlResponse>(cancellationToken: ct);
    }

    public async Task<CompleteFileUploadResponse> CompleteFileUploadAsync(CompleteFileUploadRequest request, CancellationToken ct)
    {
        await AddAuthHeaderAsync(ct);

        var response = await _httpClient.PostAsJsonAsync(
            $"{_config["MediaServiceUrl"]}/file/complete_upload",
            request,
            ct
        );

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CompleteFileUploadResponse>(cancellationToken: ct);
        if (result is null)
            throw new InvalidOperationException("Failed to deserialize CompleteFileUploadResponse.");

        return result;
    }

    public async Task DeleteFileReferenceAsync(Guid referenceId, CancellationToken ct)
    {
        await AddAuthHeaderAsync(ct);

        var response = await _httpClient.DeleteAsync(
            $"{_config["MediaServiceUrl"]}/file/reference/{referenceId}",
            ct
        );

        response.EnsureSuccessStatusCode();
    }
}
