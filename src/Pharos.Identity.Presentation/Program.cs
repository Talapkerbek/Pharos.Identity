using System.Reflection;
using Pharos.Identity.Infra;
using Pharos.Identity.Infra.Auth;
using Pharos.Identity.Infra.Data;
using Pharos.Identity.Infra.HostedServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalizedRazor();

builder.Host.AddWolverineWithAssemblyDiscovery(typeof(Program).Assembly);

builder.Services
    .AddEFDbContext(builder.Configuration)
    .AddAspIdentity(builder.Configuration)
    .AddIdentityServer(builder.Configuration)
    .AddAuthenticationAndAuthorization(builder.Configuration);

builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseDeveloperExceptionPage();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseStaticFiles();
// app.UseHttpsRedirection();

app.UseAuthorization();


app.MapControllers();
app.MapRazorPages();

app.Run();

