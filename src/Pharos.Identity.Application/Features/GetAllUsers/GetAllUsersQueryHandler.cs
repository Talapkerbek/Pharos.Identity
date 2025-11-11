using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pharos.Identity.Application.DTOs;
using Pharos.Identity.Infra.Data;
using Pharos.Identity.Infra.Redis.CacheServices;

namespace Pharos.Identity.Application.Features.GetAllUsers;

public class GetAllUsersQueryHandler()
{
    public async Task<List<UserDto>> Handle(GetAllUsersQuery query, UserManager<ApplicationUser> manager, IUserCacheService  cache)
    {
        var usersFromDb = await manager.Users.Select(u => u.ToUserDto()).ToListAsync();
        
        return usersFromDb;
    }
}