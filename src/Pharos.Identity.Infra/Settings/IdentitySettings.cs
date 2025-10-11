namespace Pharos.Identity.Infra.Settings;

public class IdentitySettings
{
    public string AdminUserEmail { get; set; } = String.Empty;
    public string AdminUserPassword { get; set; } = String.Empty;
    
    public string ClientAppUrl { get; set; } = String.Empty;
}