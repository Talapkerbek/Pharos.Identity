using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Pharos.Identity.Infra.Data;

public class ApplicationUser : IdentityUser
{
    [MaxLength(100)]
    public string FirstName { get; set; } = String.Empty;
    
    [MaxLength(100)]
    public string LastName { get; set; } = String.Empty;
    
    public string AvatarPath { get; set; } = String.Empty;
    public Guid? AvatarFileId { get; set; } = Guid.Empty;
    public Guid? TenantId { get; set; }
}