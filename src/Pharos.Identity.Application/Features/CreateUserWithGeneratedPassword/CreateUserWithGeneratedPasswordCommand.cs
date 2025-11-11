namespace Pharos.Identity.Application.Features.CreateUserWithGeneratedPassword;

public record CreateUserWithGeneratedPasswordCommand(string FirstName, string LastName, string Email, Guid? TenantId);