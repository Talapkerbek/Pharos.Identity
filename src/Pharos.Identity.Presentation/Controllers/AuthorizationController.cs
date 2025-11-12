using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using Pharos.Identity.Application.Features.GetUserInfo;
using Pharos.Identity.Infra.Data;
using Wolverine;

namespace Pharos.Identity.Presentation.Controllers;

public class AuthorizationController(IMessageBus messageBus) : Controller
{
    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // Retrieve the user principal stored in the authentication cookie.
        var result = await HttpContext.AuthenticateAsync("Identity.Application");

        // If the user principal can't be extracted, redirect the user to the login page.
        if (!result.Succeeded)
        {
            return Challenge(
                authenticationSchemes: "Identity.Application",
                properties: new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                        Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                });
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(result.Principal.Claims.Count());
        Console.WriteLine(string.Join("\n", result.Principal.Claims));
        Console.ResetColor();
        
        // Create a new claims principal
        var claims = new List<Claim>
        {
            // 'subject' claim which is required
            new Claim(OpenIddictConstants.Claims.Subject, result.Principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "NotFound"),
            new Claim("firstName", result.Principal.FindFirstValue("FirstName") ?? "NotFound").SetDestinations(OpenIddictConstants.Destinations.IdentityToken),
            new Claim("lastName", result.Principal.FindFirstValue("LastName") ?? "NotFound").SetDestinations(OpenIddictConstants.Destinations.IdentityToken),
            new Claim("email", result.Principal.FindFirstValue("email") ?? "NotFound").SetDestinations(OpenIddictConstants.Destinations.IdentityToken),
            new Claim("image", result.Principal.FindFirstValue("image") ?? "NotFound").SetDestinations(OpenIddictConstants.Destinations.IdentityToken),
        };
        
        var roleClaims = result.Principal.FindAll("roles");

        foreach (var roleClaim in roleClaims)
        {
            claims.Add(new Claim(OpenIddictConstants.Claims.Role, roleClaim.Value)
                .SetDestinations(OpenIddictConstants.Destinations.IdentityToken, OpenIddictConstants.Destinations.AccessToken));
        }

        var claimsIdentity = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        // Set requested scopes and audiences (this is not done automatically)
        claimsPrincipal.SetScopes(request.GetScopes());
        
        var scopeManager = HttpContext.RequestServices.GetRequiredService<IOpenIddictScopeManager>();

        var audiences = new HashSet<string>();

        foreach (var scope in request.GetScopes())
        {
            var scopeDescriptor = await scopeManager.FindByNameAsync(scope);
            if (scopeDescriptor is not null)
            {
                var resources = await scopeManager.GetResourcesAsync(scopeDescriptor);
                audiences.UnionWith(resources);
            }
        }

        claimsPrincipal.SetAudiences(audiences);

        // Signing in with the OpenIddict authentiction scheme trigger OpenIddict to issue a code (which can be exchanged for an access token)
        return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
    
    [HttpPost("~/connect/token")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        ClaimsPrincipal? claimsPrincipal;

        if (request.IsAuthorizationCodeGrantType())
        {
            claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
        }
        else if (request.IsClientCredentialsGrantType())
        {
            var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            claimsPrincipal = new ClaimsPrincipal(identity);
    
            identity.AddClaim(new Claim("client_id", request.ClientId ?? string.Empty));
            identity.AddClaim(OpenIddictConstants.Claims.Subject, request.ClientId ?? string.Empty);
            
            claimsPrincipal.SetScopes(request.GetScopes());
        
            var scopeManager = HttpContext.RequestServices.GetRequiredService<IOpenIddictScopeManager>();

            var audiences = new HashSet<string>();

            foreach (var scope in request.GetScopes())
            {
                var scopeDescriptor = await scopeManager.FindByNameAsync(scope);
                if (scopeDescriptor is not null)
                {
                    var resources = await scopeManager.GetResourcesAsync(scopeDescriptor);
                    audiences.UnionWith(resources);
                }
            }
            
            claimsPrincipal.SetAudiences(audiences);
        }
        else if (request.IsRefreshTokenGrantType())
        {
            claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
        }
        else
        {
            return BadRequest(new { error = "unsupported_grant_type" });
        }

        if (claimsPrincipal == null) claimsPrincipal = new ClaimsPrincipal();
        
        // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
        return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
   
    
    [HttpGet("~/connect/endsession")]
    [HttpPost("~/connect/endsession")]
    [IgnoreAntiforgeryToken]
    public async Task Logout()
    {
        await HttpContext.SignOutAsync("Identity.Application");
        await HttpContext.SignOutAsync("OpenIddict.Server.AspNetCore");
        //Important, this method should never return anything.
    }
    
    [HttpGet("~/connect/userinfo")]
    [HttpPost("~/connect/userinfo")]
    [Authorize(AuthenticationSchemes = "OpenIddict.Validation.AspNetCore")]
    public async Task<ActionResult<UserInfoDTO>> GetUserInfo()
    {
        var userId = User.FindFirstValue(OpenIddictConstants.Claims.Subject);
        if (userId is null) return BadRequest();
      
        var res = await messageBus.InvokeAsync<UserInfoDTO>(new GetUserInfoRequest(userId));
        
        return Ok(res);
    }
}