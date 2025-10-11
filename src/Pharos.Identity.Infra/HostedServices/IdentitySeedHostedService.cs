using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Pharos.Identity.Contracts;
using Pharos.Identity.Infra.Data;
using Pharos.Identity.Infra.Settings;
using Wolverine;

namespace Pharos.Identity.Infra.HostedServices;

public class IdentitySeedHostedService : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IdentitySettings _settings;

    public IdentitySeedHostedService(IServiceScopeFactory serviceScopeFactory, IOptions<IdentitySettings> options)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _settings = options.Value;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("Seeding...");
        Console.ResetColor();
        
        using var scope = _serviceScopeFactory.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        
        await CreateRoleIfNotExistAsync(Roles.SuperAdmin, roleManager);

        var adminUser = await userManager.FindByEmailAsync(_settings.AdminUserEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser()
            {
                UserName = "admin@gmail.com",
                Email = "admin@gmail.com",
                SecurityStamp = Guid.NewGuid().ToString(),
                EmailConfirmed = true,
                FirstName = "Админбек",
                LastName = "Админович"
            };

            var isSuccess = await userManager.CreateAsync(adminUser, _settings.AdminUserPassword);
            
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(isSuccess.Succeeded);
            foreach (var error in isSuccess.Errors)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {error.Code} - {error.Description}");
            }
            Console.ResetColor();
            Console.ResetColor();
            
            await userManager.AddToRoleAsync(adminUser, Roles.SuperAdmin);
        }
        
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("Seeding Finished!");
        
        await publishEndpoint.PublishAsync(new UserCreatedEvent(Guid.Parse(adminUser.Id), adminUser.FirstName, adminUser.LastName, adminUser.Email ?? ""));
        
        Console.ResetColor();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public static async Task CreateRoleIfNotExistAsync(string role, RoleManager<ApplicationRole> roleManager)
    {
        var roleExist = await roleManager.RoleExistsAsync(role);

        if (!roleExist)
        {
            await roleManager.CreateAsync(new ApplicationRole { Name = role });
        }
    }
}