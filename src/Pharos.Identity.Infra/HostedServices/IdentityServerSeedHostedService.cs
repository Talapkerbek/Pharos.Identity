using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;
using Pharos.Identity.Infra.Data;
using Pharos.Identity.Infra.Settings;

namespace Pharos.Identity.Infra.HostedServices;

public class IdentityServerSeedHostedService(IServiceProvider serviceProvider, IOptions<OAuthSettings> oauthSettings)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Seeding IdentityServer...");
            
            
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);

            var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
            var scopeManager = scope.ServiceProvider.GetRequiredService<OpenIddictScopeManager<OpenIddictEntityFrameworkCoreScope>>();
            
            await AddClients(manager, cancellationToken).ConfigureAwait(false);
            await AddScopes(scopeManager, cancellationToken).ConfigureAwait(false);
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Seeding IdentityServer FINISHED...");
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    
        
        public async Task AddClients(IOpenIddictApplicationManager manager, CancellationToken cancellationToken)
        {
            foreach (var client in oauthSettings.Value.Clients)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("ADDING CLIENTS...");
                var existingClient = await manager.FindByClientIdAsync(client.ClientId, cancellationToken)
                    .ConfigureAwait(false);

                if (existingClient != null)
                {
                    await manager.DeleteAsync(existingClient, cancellationToken).ConfigureAwait(false);
                }

                if (true)
                {
                    var clientDescriptor = new OpenIddictApplicationDescriptor
                    {
                        ClientId = client.ClientId,
                        ClientSecret = client.ClientSecret,
                        DisplayName = client.DisplayName
                    };
                    
                    foreach (var redirectUri in client.RedirectUris)
                    {
                        clientDescriptor.RedirectUris.Add(new Uri(redirectUri));
                    }
                    
                    foreach (var postLogoutRedirectUri in client.PostLogoutRedirectUris)
                    {
                        clientDescriptor.PostLogoutRedirectUris.Add(new Uri(postLogoutRedirectUri));
                    }
                    
                    foreach (var scope in client.AllowedScopes)
                    {
                        clientDescriptor.AddScopePermissions(scope);
                    }
                    
                    foreach (var permission in client.Permissions)
                    {
                        clientDescriptor.Permissions.Add(permission);
                    }

                    if (client.RequirePKCE)
                    {
                        clientDescriptor.Requirements.Add(OpenIddictConstants.Requirements.Features
                            .ProofKeyForCodeExchange);
                    }
                    
                    await manager.CreateAsync(clientDescriptor, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public async Task AddScopes(OpenIddictScopeManager<OpenIddictEntityFrameworkCoreScope> scopeManager, CancellationToken cancellationToken)
        {
            foreach (var scope in oauthSettings.Value.Scopes)
            {
                if (await scopeManager.FindByNameAsync(scope.Name, cancellationToken)
                        .ConfigureAwait(false) == null)
                {
                    OpenIddictScopeDescriptor descriptor = new OpenIddictScopeDescriptor {
                        DisplayName = scope.DisplayName,
                        Name = scope.Name
                    };
                    
                    foreach (var resource in scope.Resources)
                    {
                        descriptor.Resources.Add(resource);
                    }
                    
                    await scopeManager.CreateAsync(descriptor, cancellationToken).ConfigureAwait(false);
                }
            }
        }
}