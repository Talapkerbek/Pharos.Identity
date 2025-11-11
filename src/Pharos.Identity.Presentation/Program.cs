using Pharos.Identity.Application.Features.GetPresignedUrlForAvatarLogo;
using Pharos.Identity.Application.Services.MediaClient;
using Pharos.Identity.Application.Services.TokenService;
using Pharos.Identity.Infra;
using Pharos.Identity.Infra.Auth;
using Pharos.Identity.Infra.Data;
using Pharos.Identity.Infra.Logging;
using Pharos.Identity.Infra.Redis;
using Pharos.Identity.Presentation.AppExtensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureCors();

builder.Services.AddLocalizedRazor();
builder.Services
    .AddMemoryCache()
    .AddRedisCache(builder.Configuration)
    .AddTokenService()
    .AddMediaServiceClient()
    .AddAndConfigureSerilog(builder.Configuration);

builder.Host
    .AddWolverineWithAssemblyDiscovery(builder.Configuration, [typeof(GetPresignedUrlForAvatarLogoHandler).Assembly]);

builder.Services
    .AddEFDbContext(builder.Configuration)
    .AddAspIdentity(builder.Configuration)
    .AddIdentityServer(builder.Configuration)
    .AddAuthenticationAndAuthorization(builder.Configuration);

builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseDeveloperExceptionPage();
app.UseRequestLocalization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
// app.UseHttpsRedirection();

app.UseRouting();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();