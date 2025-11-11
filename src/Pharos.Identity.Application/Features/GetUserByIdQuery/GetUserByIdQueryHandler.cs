using Microsoft.AspNetCore.Identity;
using Pharos.Identity.Application.DTOs;
using Pharos.Identity.Infra.Data;
using Pharos.Identity.Infra.Redis.CacheServices;

namespace Pharos.Identity.Application.Features.GetUserByIdQuery;

public class GetUserByIdQueryHandler()
{
    public async Task<UserDto?> Handle(GetUserByIdQuery query, UserManager<ApplicationUser> manager, IUserCacheService  cache)
    {
        var userFromCache = await cache.GetUserAsync(query.Id);

        if (userFromCache != null) return userFromCache;
        
        var userFromDb = await manager.FindByIdAsync(query.Id);
        if (userFromDb == null) return null;
        
        var userDto = userFromDb.ToUserDto();
        
        await cache.SetUserAsync(userDto, TimeSpan.FromMinutes(15));

        return userDto;
    }
}