using Pharos.Identity.Infra.Data;

namespace Pharos.Identity.Application.DTOs;

public static class ApplicationUserMapper
{
    public static UserDto ToUserDto(this ApplicationUser user)
    {
        return new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.AvatarPath, user.TenantId);
    }
}