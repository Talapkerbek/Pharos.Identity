using Pharos.Media.Contracts;

namespace Pharos.Identity.Application.Services.MediaClient;

public interface IMediaServiceClient
{
    Task<GetPresignedUrlResponse?> GetPresignedUrlAsync(GetPresignedUrlRequest request, CancellationToken ct);
    Task<CompleteFileUploadResponse> CompleteFileUploadAsync(CompleteFileUploadRequest request, CancellationToken ct);
    Task DeleteFileReferenceAsync(Guid referenceId, CancellationToken ct);
}