namespace Pharos.Identity.Application.Features.GetPresignedUrlForAvatarLogo;

public record GetPresignedUrlForAvatarLogoCommand
( 
    Guid UserId,
    long Size,
    string Checksum,
    string ContentType,
    Guid UploaderId
);