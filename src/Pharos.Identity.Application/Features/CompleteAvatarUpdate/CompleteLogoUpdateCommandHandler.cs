using Microsoft.AspNetCore.Identity;
using Pharos.Identity.Application.Services.MediaClient;
using Pharos.Identity.Infra.Data;
using Pharos.Identity.Infra.Exceptions;
using Pharos.Media.Contracts;

namespace Pharos.Identity.Application.Features.CompleteAvatarUpdate;

public class CompleteAvatarUpdateHandler
{
    public async Task Handle(
        CompleteAvatarUpdateCommand command,
        UserManager<ApplicationUser> userManager,
        IMediaServiceClient mediaClient,
        CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null) throw new NotFoundException($"User with Id {command.UserId} not found");

        var result = await mediaClient.CompleteFileUploadAsync(
            new CompleteFileUploadRequest(command.UploadId),
            ct
        );

        user.AvatarPath = result.BlobPath;
        user.AvatarFileId = result.ReferenceId;
        
        await userManager.UpdateAsync(user);
    }
}