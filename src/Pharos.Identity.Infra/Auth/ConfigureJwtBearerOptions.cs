using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Pharos.Identity.Infra.Settings;

namespace Pharos.Identity.Infra.Auth;

public class ConfigureJwtBearerOptions(IConfiguration configuration) : IConfigureNamedOptions<JwtBearerOptions>
{
    
    public void Configure(JwtBearerOptions options)
    {
        Configure(Options.DefaultName, options);
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        if (name == JwtBearerDefaults.AuthenticationScheme)
        {
            var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>()!;
            
            options.Authority = serviceSettings.Authority;
            options.Audience = serviceSettings.ServiceName;
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                NameClaimType = "name",
                RoleClaimType = "role"
            };
            
            /*options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.HasValue &&
                    path.StartsWithSegments("/notifications/hubs/chat"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };*/
        }
    }
}