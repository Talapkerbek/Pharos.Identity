namespace Pharos.Identity.Application.Features.ChangeName;

public record ChangeNameCommand(Guid UserId, string FirstName, string LastName);