using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pharos.Identity.Infra.Data;
using Pharos.Identity.Infra.HostedServices;
using Pharos.Identity.Infra.Settings;

namespace Pharos.Identity.Infra.Auth;


public static class IdentityServerExtension
{
    
    public static IServiceCollection AddIdentityServer(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(nameof(OAuthSettings)).Get<OAuthSettings>();

        if (settings == null)
        {
            throw new NullReferenceException("OAuth settings not found. Please be sure that you provide \"OAuth\" settings in the configuration.");
        }
        
        services.AddOpenIddict()
            .AddCore(opt =>
            {
                opt.UseEntityFrameworkCore()
                    .UseDbContext<ApplicationDbContext>();
            })
            .AddServer(opt =>
            {
                opt.SetIssuer(settings.Issuer);
                
                // TODO: Change with real certificates
                opt
                    .AddEphemeralEncryptionKey()
                    .AddEphemeralSigningKey()
                    .DisableAccessTokenEncryption();
                
                opt
                    .SetAuthorizationEndpointUris("/connect/authorize")
                    .SetTokenEndpointUris("/connect/token");
                
                opt.SetEndSessionEndpointUris("/connect/endsession")
                    .SetUserInfoEndpointUris("/connect/userinfo");
                
                opt.AllowAuthorizationCodeFlow()
                    .RequireProofKeyForCodeExchange();

                opt.AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();

                opt.UseAspNetCore()
                    .EnableTokenEndpointPassthrough()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough()
                    // TODO: In production, this option must be removed, and all the requests must be made through HTTPS. Just add nginx headers forwarder.
                    .DisableTransportSecurityRequirement();
            })
            .AddValidation(opt =>
                {
                    opt.UseLocalServer();
                    opt.UseAspNetCore();
                    opt.AddAudiences("IdentityServer");
                }
            );
        
        services.Configure<OAuthSettings>(configuration.GetSection(nameof(OAuthSettings)));
        services.AddHostedService<IdentityServerSeedHostedService>();
        
        return services;
    }
    
}