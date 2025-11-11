using Microsoft.AspNetCore.Identity;
using Pharos.Identity.Infra.Data;
using Pharos.Identity.Infra.Exceptions;

namespace Pharos.Identity.Application.Features.ResetPassword;

public class ResetPasswordHandler
{
    public async Task<ResetPasswordResponse> Handle(ResetPasswordCommand command, UserManager<ApplicationUser> userManager)
    { 
        var user = await userManager.FindByIdAsync(command.UserId);
        if (user == null) 
            throw new NotFoundException($"User with Id {command.UserId} not found");
        
        var temporaryPassword = GenerateTemporaryPassword();
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, temporaryPassword);
        
        return new (temporaryPassword);
    }
    
    public static string GenerateTemporaryPassword()
    {
        var random = new Random();

        return random.Next(1_0000_0000, 9_9999_9999).ToString();
    }
}