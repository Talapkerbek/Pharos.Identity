using Microsoft.AspNetCore.Identity;
using Pharos.Identity.Infra.Data;
using Pharos.Identity.Infra.Exceptions;

namespace Pharos.Identity.Application.Features.ChangeName;

public class ChangeNameHandler
{
    public async Task Handle(UserManager<ApplicationUser> userManager, ChangeNameCommand command)
    {
        var user = await userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null) throw new NotFoundException($"User with Id {command.UserId} not found");

        user.FirstName = command.FirstName;
        user.LastName = command.LastName;
        
        await userManager.UpdateAsync(user);
    }
}