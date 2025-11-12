using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity.UI.Services;
using Resend;

namespace Pharos.Identity.Infra.Email;

public static class Extension
{
    public static IServiceCollection AddEmailSender(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<ResendClient>();
        services.Configure<ResendClientOptions>( o =>
        {
            o.ApiToken = configuration["Resend:ApiToken"]!;
        } );
        services.AddTransient<IResend, ResendClient>();
        
        services.AddTransient<IEmailSender, ResendEmailSender>();
        
        return services;
    }
}