namespace Pharos.Identity.Application.Features.GetUserInfo;

public record UserInfoDTO(string Id, string FirstName, string LastName, string Email, string AvatarPath, List<string> Roles, string UserName);
