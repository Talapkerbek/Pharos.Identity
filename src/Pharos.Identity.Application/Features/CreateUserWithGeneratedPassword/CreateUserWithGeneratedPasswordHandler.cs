using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Pharos.Identity.Infra.Data;
using Pharos.Identity.Infra.Exceptions;

namespace Pharos.Identity.Application.Features.CreateUserWithGeneratedPassword;

public class CreateUserWithGeneratedPasswordHandler
{
    public async Task<CreateUserWithGeneratedPasswordResponse> Handle(CreateUserWithGeneratedPasswordCommand command, UserManager<ApplicationUser>  userManager, ILogger<CreateUserWithGeneratedPasswordHandler> logger)
    {
        var existingUser = await userManager.FindByEmailAsync(command.Email);

        if (existingUser is not null)
            throw new BadRequestException($"User with email:{command.Email} already exists");

        var user = new ApplicationUser()
        {
            Id = Guid.NewGuid().ToString(),
            FirstName = command.FirstName,
            LastName = command.LastName,
            UserName = command.Email,
            Email = command.Email,
            TenantId = command.TenantId
        };
        
        var temporaryPassword = GenerateTemporaryPassword();

        var res = await userManager.CreateAsync(user, temporaryPassword);

        if (!res.Succeeded)
        {
            logger.LogError("Failed to create user, errors: {@Errors}", res.Errors);
            throw new BadRequestException($"User creation failed.");
        }
        
        return new CreateUserWithGeneratedPasswordResponse(user.Id, temporaryPassword);
    }

    public static string GenerateTemporaryPassword()
    {
        var random = new Random();

        return random.Next(1_0000_0000, 9_9999_9999).ToString();
    }
}