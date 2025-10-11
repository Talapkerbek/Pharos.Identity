/*using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using OpenIddict.Abstractions;
using Talapker.Identity.Application.DTOs;
using Talapker.Identity.Contracts;
using Talapker.Identity.Infra.Data;
using Wolverine;

namespace Talapker.Identity.Service.Controllers;

[ApiController]
[Route("api/user")]
public class UserController
(
    UserManager<ApplicationUser> userManager,
    IMessageBus publishEndpoint,
    ApplicationDbContext dbContext
) : ControllerBase
{
    [HttpGet("HOST")]
    public ActionResult GET()
    {
        return Ok(new {
            Scheme = Request.Scheme,
            Host = Request.Host.Value,
            Path = Request.Path,
            DisplayUrl = Request.GetDisplayUrl().ToString(),
        });
    }
    
    // Should make it paginated in the future
    [HttpGet]
    [Authorize(AuthenticationSchemes = "OpenIddict.Validation.AspNetCore")]
    public async Task<ActionResult<List<UserInfoDTO>>> GetAllAsync()
    {
        var users = await userManager.Users.ToListAsync().ConfigureAwait(false);

        var zavuches = (await userManager.GetUsersInRoleAsync(Roles.Zavuch).ConfigureAwait(false)).Select(u => u.Id).ToHashSet();
        var teachers = (await userManager.GetUsersInRoleAsync(Roles.Teacher).ConfigureAwait(false)).Select(u => u.Id).ToHashSet();
        var admins = (await userManager.GetUsersInRoleAsync(Roles.Admin).ConfigureAwait(false)).Select(u => u.Id).ToHashSet();

        var userInfoDtos = users.Select(user =>
        {
            var roles = new List<string>();

            if (zavuches.Contains(user.Id)) roles.Add(Roles.Zavuch);
            if (teachers.Contains(user.Id)) roles.Add(Roles.Teacher);
            if (admins.Contains(user.Id)) roles.Add(Roles.Admin);
            
            return
                new UserInfoDTO(
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email ?? "",
                    user.AvatarPath,
                    roles,
                    user.IsActive,
                    user.UserName ?? ""
                );
        }).ToList();
        
        return Ok(userInfoDtos);
    }#2#

    [HttpGet("{id:guid}")]
    [Authorize(AuthenticationSchemes = "OpenIddict.Validation.AspNetCore")]
    public async Task<ActionResult<UserInfoDTO>> GetByIdAsync(Guid id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return NotFound();
        }

        var roles = new List<string>();
        if (await userManager.IsInRoleAsync(user, Roles.Zavuch)) roles.Add(Roles.Zavuch);
        if (await userManager.IsInRoleAsync(user, Roles.Teacher)) roles.Add(Roles.Teacher);
        if (await userManager.IsInRoleAsync(user, Roles.Admin)) roles.Add(Roles.Admin);

        var dto = new UserInfoDTO(
            user.Id,
            user.Email ?? "",
            user.FirstName,
            user.LastName
        );

        return Ok(dto);
    }
    
    [HttpPost("create")]
    [Authorize(AuthenticationSchemes = "OpenIddict.Validation.AspNetCore", Roles = $"{Roles.Zavuch},{Roles.Admin}")]
    public async Task<ActionResult<CreateUserResponse>> CreateAsync([FromBody] CreateUserRequest createUserRequest)
    {
        var res = await userService.CreateUser(createUserRequest).ConfigureAwait(false);

        return Ok(res);
    }
    
    [HttpPut("{id:guid}/reset")]
    [Authorize(AuthenticationSchemes = "OpenIddict.Validation.AspNetCore", Roles = $"{Roles.Zavuch},{Roles.Admin}")]
    public async Task<ActionResult<ResetUserPasswordDTO>> ResetAsync(Guid id)
    {
        var user = await userManager.FindByIdAsync(id.ToString()).ConfigureAwait(false);
        
        if (user == null)
        {
            return BadRequest("Пользователь не найден");
        }
        
        if (await userManager.IsInRoleAsync(user, Roles.Admin).ConfigureAwait(false))
        {
            return BadRequest("Нельзя менять пароль админа.");
        }

        var password = Random.Shared.NextInt64(10_0000, 100_0000).ToString();
        var token = await userManager.GeneratePasswordResetTokenAsync(user).ConfigureAwait(false);
        await userManager.ResetPasswordAsync(user, token, password).ConfigureAwait(false);

        return Ok(new ResetUserPasswordDTO(user.UserName ?? "", password));
    }
    
    [HttpPut("role/assign")]
    [Authorize(AuthenticationSchemes = "OpenIddict.Validation.AspNetCore", Roles = $"{Roles.Zavuch},{Roles.Admin}")]
    public async Task<ActionResult> AssignToRoleAsync([FromBody] UserAssignRoleRequest assignRoleRequest)
    {
        var user = await userManager.FindByIdAsync(assignRoleRequest.UserId.ToString()).ConfigureAwait(false);
        
        if (user == null)
        {
            return BadRequest("Пользователь не найден");
        }
        
        if (assignRoleRequest.Role == Roles.Teacher)
        {
            await publishEndpoint.Publish(new TeacherRoleAssignedEvent(Guid.Parse(user.Id))).ConfigureAwait(false);
        }
        
        if (assignRoleRequest.Role == Roles.Zavuch)
        {
            await publishEndpoint.Publish(new ZavuchRoleAssignedEvent(Guid.Parse(user.Id))).ConfigureAwait(false);
        }
        
        await userManager.AddToRoleAsync(user, assignRoleRequest.Role).ConfigureAwait(false);
        var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);

        var dto = new UserInfoDTO(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email ?? "",
            user.AvatarPath,
            roles.ToList(),
            user.IsActive,
            user.UserName ?? ""
        );
        
        await cacheService.SetUserAsync(dto);
            
        await dbContext.SaveChangesAsync().ConfigureAwait(false);
        
        return Ok();
    }
    
    [HttpPut("{id:guid}/deactivate")]
    [Authorize(AuthenticationSchemes = "OpenIddict.Validation.AspNetCore", Roles = $"{Roles.Zavuch},{Roles.Admin}")]
    public async Task<ActionResult> DeactivateAsync(Guid id)
    {
        var user = await userManager.FindByIdAsync(id.ToString()).ConfigureAwait(false);

        if (user == null)
        {
            return BadRequest("Пользователь не найден");
        }
        
        if (await userManager.IsInRoleAsync(user, Roles.Admin).ConfigureAwait(false))
        {
            return BadRequest("Админа нельзя удалять.");
        }
        
        user.IsActive = false;

        if (await userManager.IsInRoleAsync(user, Roles.Teacher).ConfigureAwait(false))
        {
            await userManager.RemoveFromRoleAsync(user,Roles.Teacher).ConfigureAwait(false);
            await publishEndpoint.Publish(new TeacherRoleDeassignedEvent(Guid.Parse(user.Id))).ConfigureAwait(false);
        }
        
        await userManager.UpdateAsync(user).ConfigureAwait(false);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);
        
        var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);

        var dto = new UserInfoDTO(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email ?? "",
            user.AvatarPath,
            roles.ToList(),
            user.IsActive,
            user.UserName ?? ""
        );

        await cacheService.SetUserAsync(dto);
        
        return Ok();
    }
    
    [HttpPut("{id:guid}/activate")]
    [Authorize(AuthenticationSchemes = "OpenIddict.Validation.AspNetCore", Roles = $"{Roles.Zavuch},{Roles.Admin}")]
    public async Task<ActionResult> ActivateAsync(Guid id)
    {
        var user = await userManager.FindByIdAsync(id.ToString()).ConfigureAwait(false);
        
        if (user == null)
        {
            return BadRequest("Пользователь не найден");
        }
        
        user.IsActive = true;
        
        await userManager.UpdateAsync(user).ConfigureAwait(false);
        
        var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);

        var dto = new UserInfoDTO(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email ?? "",
            user.AvatarPath,
            roles.ToList(),
            user.IsActive,
            user.UserName ?? ""
        );

        await cacheService.SetUserAsync(dto);
        
        await dbContext.SaveChangesAsync().ConfigureAwait(false);
        
        return Ok();
    }
    
    [HttpPut("role/deassign")]
    [Authorize(AuthenticationSchemes = "OpenIddict.Validation.AspNetCore", Roles = $"{Roles.Zavuch},{Roles.Admin}")]
    public async Task<ActionResult> DeassignToRoleAsync([FromBody] UserAssignRoleRequest assignRoleRequest)
    {
        var user = await userManager.FindByIdAsync(assignRoleRequest.UserId.ToString()).ConfigureAwait(false);
        
        if (user == null)
        {
            return BadRequest("Пользователь не найден");
        }
        
        if (await userManager.IsInRoleAsync(user, Roles.Admin).ConfigureAwait(false))
        {
            return BadRequest("Админа нельзя менять.");
        }


        if (await userManager.IsInRoleAsync(user, assignRoleRequest.Role).ConfigureAwait(false))
        {
            await userManager.RemoveFromRoleAsync(user, assignRoleRequest.Role).ConfigureAwait(false);

            if (assignRoleRequest.Role == Roles.Teacher)
            {
                await publishEndpoint.Publish(new TeacherRoleDeassignedEvent(Guid.Parse(user.Id))).ConfigureAwait(false);
            }
            
            if (assignRoleRequest.Role == Roles.Zavuch)
            {
                await publishEndpoint.Publish(new ZavuchRoleDeassignedEvent(Guid.Parse(user.Id))).ConfigureAwait(false);
            }
            
            var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);

            var dto = new UserInfoDTO(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email ?? "",
                user.AvatarPath,
                roles.ToList(),
                user.IsActive,
                user.UserName ?? ""
            );

            await cacheService.SetUserAsync(dto);

            
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        else
        {
            return BadRequest($"Пользователь и так не находится в роли {assignRoleRequest.Role}.");
        }
        
        return Ok();
    }

    [HttpPut]
    [Authorize(AuthenticationSchemes = "OpenIddict.Validation.AspNetCore")]
    public async Task<ActionResult> ChangeName([FromBody] ChangeUserNameDTO changeUserNameDro)
    {
        if (!Guid.TryParse(User.FindFirstValue(OpenIddictConstants.Claims.Subject), out var sub))
        {
            return BadRequest();
        }
        
        var user  =  await userManager.FindByIdAsync(sub.ToString()).ConfigureAwait(false);
        
        if (user == null) return NoContent();
        
        user.FirstName = changeUserNameDro.FirstName;
        user.LastName = changeUserNameDro.LastName;
        
        await userManager.UpdateAsync(user).ConfigureAwait(false);
        
        var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);

        var dto = new UserInfoDTO(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email ?? "",
            user.AvatarPath,
            roles.ToList(),
            user.IsActive,
            user.UserName ?? ""
        );

        await cacheService.SetUserAsync(dto);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);
        
        return Ok();
    }
    
    [HttpPut("avatar")]
    [Authorize(AuthenticationSchemes = "OpenIddict.Validation.AspNetCore")]
    public async Task<ActionResult> ChangeAvatar([FromBody] ChangeAvatarRequest changeAvatarRequest)
    {
        if (!Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub),  out var sub)) return BadRequest();
        
        var user  =  await userManager.FindByIdAsync(sub.ToString()).ConfigureAwait(false);
        if (user == null) return BadRequest("User not found.");
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(user.AvatarFileId);
        if (user.AvatarFileId is not null)
        {
            await publishEndpoint.Publish(new FileDeattachedEvent(user.AvatarFileId.Value)).ConfigureAwait(false);
        }

        user.AvatarPath = changeAvatarRequest.FileKey;
        user.AvatarFileId = changeAvatarRequest.FileId;
        
        await publishEndpoint.Publish(new FileAttachedEvent(changeAvatarRequest.FileId)).ConfigureAwait(false);
        await publishEndpoint.Publish(new UserAvatarChangedEvent(Guid.Parse(user.Id), user.AvatarPath)).ConfigureAwait(false);
        
        await userManager.UpdateAsync(user).ConfigureAwait(false);
        
        var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);

        var dto = new UserInfoDTO(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email ?? "",
            user.AvatarPath,
            roles.ToList(),
            user.IsActive,
            user.UserName ?? ""
        );

        await cacheService.SetUserAsync(dto);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        return Ok();
    }
    
    [HttpDelete("{userId:guid}/avatar")]
    [Authorize(AuthenticationSchemes = "OpenIddict.Validation.AspNetCore")]
    public async Task<ActionResult> DeleteAvatar()
    {
        if (!Guid.TryParse(User.FindFirstValue(OpenIddictConstants.Claims.Subject),  out var sub)) return BadRequest();
        
        var user  =  await userManager.FindByIdAsync(sub.ToString()).ConfigureAwait(false);
        if (user == null) return BadRequest("User not found.");


        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(user.AvatarFileId);
        if (user.AvatarFileId is not null)
        {
            Console.WriteLine("YEAHH");
            await publishEndpoint.Publish(new FileDeattachedEvent(user.AvatarFileId.Value)).ConfigureAwait(false);
            await publishEndpoint.Publish(new UserAvatarChangedEvent(Guid.Parse(user.Id), "")).ConfigureAwait(false);
        }
        
        user.AvatarPath = "";
        user.AvatarFileId = null;
        
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        return Ok();
    }
}*/