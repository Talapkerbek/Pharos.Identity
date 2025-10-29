using Microsoft.AspNetCore.Identity;
using Pharos.Identity.Application.Services.MediaClient;
using Pharos.Identity.Infra.Data;
using Pharos.Identity.Infra.Exceptions;
using Pharos.Media.Contracts;
using Wolverine;

namespace Pharos.Identity.Application.Features.GetPresignedUrlForAvatarLogo;

public class GetPresignedUrlForAvatarLogoHandler
{
    public async Task<GetPresignedUrlResponse?> Handle(
        GetPresignedUrlForAvatarLogoCommand command,
        UserManager<ApplicationUser> userManager,
        IMediaServiceClient mediaClient,
        IMessageBus messageBus,
        CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null) throw new NotFoundException($"User with Id {command.UserId} not found");
        
        if (user.AvatarFileId.HasValue)
            await messageBus.PublishAsync(new DeleteFileReferenceCommand(user.AvatarFileId.Value));

        var request = new GetPresignedUrlRequest(
            command.UploaderId,
            command.Size,
            command.Checksum,
            command.ContentType
        );

        var result = await mediaClient.GetPresignedUrlAsync(request, ct);
        if (result == null) return null;

        if (result.IsBlobExists && !string.IsNullOrEmpty(result.BlobPath))
        {
            user.AvatarPath = result.BlobPath;
            user.AvatarFileId = result.ReferenceId;
        }
        
        await userManager.UpdateAsync(user); 

        return result;
    }
}