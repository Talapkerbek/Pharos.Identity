using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Pharos.Identity.Infra.Logging;

public static class Extensions
{
    public static IServiceCollection AddAndConfigureSerilog
    (
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information() 
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Marten", LogEventLevel.Warning)
            .MinimumLevel.Override("Wolverine", LogEventLevel.Warning) 
            .MinimumLevel.Override("Npgsql", LogEventLevel.Warning) 
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                theme: AnsiConsoleTheme.Sixteen 
            )
            .CreateLogger();

        services.AddSerilog();
        
        return services;
    }
}