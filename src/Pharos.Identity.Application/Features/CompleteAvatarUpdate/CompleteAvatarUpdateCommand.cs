namespace Pharos.Identity.Application.Features.CompleteAvatarUpdate;

public record CompleteAvatarUpdateCommand(Guid UserId, Guid UploadId);