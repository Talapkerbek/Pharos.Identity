using Microsoft.AspNetCore.Mvc;
using Pharos.Identity.Application.Features.CreateUserWithGeneratedPassword;
using Pharos.Identity.Application.Features.GetAllUsers;
using Pharos.Identity.Application.Features.GetUserByIdQuery;
using Pharos.Identity.Application.Features.ResetPassword;
using Pharos.Identity.Infra.Data;
using Wolverine;

namespace Pharos.Identity.Presentation.Controllers;

[ApiController]
[Route("user")]
public class UserController(IMessageBus messageBus) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(string id)
    {
        var user = await messageBus.InvokeAsync<UserDto?>(new GetUserByIdQuery(id));
        
        if (user == null) return NotFound();
        return Ok(user);
    }
    
    [HttpGet("all")]
    public async Task<ActionResult<List<UserDto>>> GetUser()
    {
        var user = await messageBus.InvokeAsync<List<UserDto>>(new GetAllUsersQuery());
        
        return Ok(user);
    }
    
    [HttpPost]
    public async Task<ActionResult<CreateUserWithGeneratedPasswordResponse>> CreateUser(CreateUserWithGeneratedPasswordCommand command)
    {
        var response = await messageBus.InvokeAsync<CreateUserWithGeneratedPasswordResponse?>(command);
        return Ok(response);
    }
    
    [HttpPost("reset")]
    public async Task<ActionResult<CreateUserWithGeneratedPasswordResponse>> ResetPassword(ResetPasswordCommand command)
    {
        var response = await messageBus.InvokeAsync<ResetPasswordResponse?>(command);
        return Ok(response);
    }
}