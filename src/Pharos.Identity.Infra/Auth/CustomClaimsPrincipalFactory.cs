using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Pharos.Identity.Infra.Data;

namespace Pharos.Identity.Infra.Auth;

public class CustomClaimsPrincipalFactory(UserManager<ApplicationUser> userManager, IOptions<IdentityOptions> optionsAccessor)
    : UserClaimsPrincipalFactory<ApplicationUser>(userManager, optionsAccessor)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        ClaimsIdentity claims = await base.GenerateClaimsAsync(user);
        var roles = await UserManager.GetRolesAsync(user);
        
        claims.AddClaim(new Claim("FirstName", user.FirstName));
        claims.AddClaim(new Claim("LastName", user.LastName));
        claims.AddClaim(new Claim("sub", user.Id));
        
        foreach (var role in roles)
        {
            claims.AddClaim(new Claim(ClaimTypes.Role, role));
            claims.AddClaim(new Claim("roles", role));
        }
        
        if (user.Email != null) claims.AddClaim(new Claim("email", user.Email));
        
        return claims;
    }
}