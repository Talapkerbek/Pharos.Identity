namespace Pharos.Identity.Infra.Data;

public record UserDto(string Id, string Email, string FirstName, string LastName, string AvatarPath, Guid? TenantId);