using Home.WebUI.Components;
using Home.WebUI.Infrastructure.HttpClients;
using Home.WebUI.Infrastructure.Security;
using Home.WebUI.Infrastructure.Services.HttpClients;
using Home.WebUI.Infrastructure.Services.Security;
using Home.WebUI.Infrastructure.UriProvider;
using Microsoft.AspNetCore.Authentication.Cookies;
using MudBlazor.Services;

var _Builder = WebApplication.CreateBuilder(args);

// Add services to the container.
_Builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

_Builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = AuthorisationUriProvider.GetLoginUri();
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

_Builder.Services.AddAuthorization();
_Builder.Services.AddHttpContextAccessor();
_Builder.Services.AddDataProtection();

_Builder.Services.AddHttpClient<IHomeHttpClient, HomeHttpClient>(options =>
{
    var _BaseUrlString = _Builder.Configuration["apiBaseUrl"];

    if (_BaseUrlString == null)
        throw new InvalidOperationException("API base URL is not configured.");
    else if (!Uri.TryCreate(_BaseUrlString, UriKind.Absolute, out var _BaseUrl))
        throw new InvalidOperationException("API base URL is malformed.");

    options.BaseAddress = new(_BaseUrlString);
})
    .AddHttpMessageHandler<TokenDelegatingHandler>();

_Builder.Services.AddScoped<TokenDelegatingHandler>();
_Builder.Services.AddScoped<IAuthorisationService, AuthorisationService>();

_Builder.Services.AddDistributedMemoryCache();
_Builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

_Builder.Services.AddMudServices();

var _App = _Builder.Build();

// Configure the HTTP request pipeline.
if (!_App.Environment.IsDevelopment())
{
    _ = _App.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    _ = _App.UseHsts();
}

_App.UseHttpsRedirection();
_App.UseStaticFiles();
_App.UseAntiforgery();

_App.UseAuthentication();
_App.UseAuthorization();

_App.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

_App.MapBlazorHub();

_App.Run();
