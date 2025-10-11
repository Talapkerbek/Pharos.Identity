namespace Pharos.Identity.Infra.Settings;

public class OAuthSettings
{
    public List<OAuthClientSettings> Clients { get; set; } = new();
    public List<Scope> Scopes { get; set; } = new();
    public string Issuer { get; set; } = String.Empty;
}

public class Scope
{
    public string DisplayName { get; set; } = String.Empty;
    public string Name { get; set; } = String.Empty;
    public List<string> Resources { get; set; } = new();
}
    
public class OAuthClientSettings
{
    public string ClientId { get; set; } = String.Empty;
    public string ClientSecret { get; set; } = String.Empty;
    public string DisplayName { get; set; } = String.Empty;
    public List<string> RedirectUris { get; set; } = new();
    public List<string> PostLogoutRedirectUris { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    public List<string> AllowedScopes { get; set; } = new();
    public bool RequirePKCE { get; set; }
}