using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pharos.Identity.Infra.Data;
using Pharos.Identity.Infra.HostedServices;
using Pharos.Identity.Infra.Settings;

namespace Pharos.Identity.Infra.Auth;

public static class AuthExtensions
{
    public static IServiceCollection AddAspIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomClaimsPrincipalFactory>();
        
        services
            .Configure<IdentitySettings>(configuration.GetSection(nameof(IdentitySettings)))
            .AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = false;           
                options.Password.RequireUppercase = false;      
                options.Password.RequireLowercase = false;     
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.SignIn.RequireConfirmedEmail = false;
                options.User.RequireUniqueEmail = true;
                
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                    "абвгдеёжзийклмнопрстуфхцчшщъыьэюя" +
                    "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ" +
                    "әғқңөұүһӘҒҚҢӨҰҮҺІі" +
                    "_1234567890@!#$%^&*()-+=.,";
            })
            .AddRoles<ApplicationRole>()
            .AddClaimsPrincipalFactory<CustomClaimsPrincipalFactory>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddHostedService<IdentitySeedHostedService>();
        
        return services;
    }

    public static IServiceCollection AddAuthenticationAndAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureOptions<ConfigureJwtBearerOptions>()
            .AddAuthentication();
        /*.AddGoogle(opt =>
        {
            opt.ClientId = configuration["Authorize:Google:Id"]!;
            opt.ClientSecret = configuration["Authorize:Google:Secret"]!;
            opt.CallbackPath = new PathString("/signin-google");

            opt.Events.OnRedirectToAuthorizationEndpoint = context =>
            {
                // форсируем домен
                var redirectUri = $"https://mektep32.org/signin-google";
                var redirectUrl = QueryHelpers.AddQueryString(
                    context.Options.AuthorizationEndpoint,
                    new Dictionary<string, string?>
                    {
                        ["client_id"] = context.Options.ClientId,
                        ["redirect_uri"] = redirectUri,
                        ["response_type"] = "code",
                        ["scope"] = string.Join(" ", context.Options.Scope),
                        ["state"] = context.Properties.Items[".xsrf"] // сохраняем state!
                    });

                context.Response.Redirect(redirectUrl);
                return Task.CompletedTask;
            };
        });*/
        
            
        services.AddAuthorization();
        
        return services;
    }
}