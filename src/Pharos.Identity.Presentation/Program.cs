using Pharos.Identity.Application.Features.GetPresignedUrlForAvatarLogo;
using Pharos.Identity.Application.Services.MediaClient;
using Pharos.Identity.Application.Services.TokenService;
using Pharos.Identity.Infra;
using Pharos.Identity.Infra.Auth;
using Pharos.Identity.Infra.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddLocalizedRazor();
builder.Services
    .AddMemoryCache()
    .AddTokenService()
    .AddMediaServiceClient();

builder.Host
    .AddWolverineWithAssemblyDiscovery(builder.Configuration, [typeof(Program).Assembly, typeof(GetPresignedUrlForAvatarLogoHandler).Assembly]);

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
app.UseRequestLocalization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
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