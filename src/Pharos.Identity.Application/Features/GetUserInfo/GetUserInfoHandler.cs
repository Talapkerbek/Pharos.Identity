using Microsoft.AspNetCore.Identity;
using Pharos.Identity.Application.Features.ChangeName;
using Pharos.Identity.Infra.Data;
using Pharos.Identity.Infra.Exceptions;

namespace Pharos.Identity.Application.Features.GetUserInfo;

public class GetUserInfoHandler
{
    public async Task<UserInfoDTO> Handle(GetUserInfoRequest request, UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null) throw new NotFoundException($"User with Id {request.UserId} not found");
        
        var roles = (await userManager.GetRolesAsync(user).ConfigureAwait(false)).ToList();

        return new UserInfoDTO(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email ?? "",
            user.AvatarPath,
            roles,
            user.UserName ?? ""
        );
    }
}