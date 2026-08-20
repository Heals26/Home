using Home.WebUI.Components;
using Microsoft.AspNetCore.DataProtection;
using Home.WebUI.Endpoints;
using Home.WebUI.Infrastructure.ChangeNotifications;
using Home.WebUI.Infrastructure.HttpClients;
using Home.WebUI.Infrastructure.Security;
using Home.WebUI.Infrastructure.Services.ChangeNotifications;
using Home.WebUI.Infrastructure.Services.HttpClients;
using Home.WebUI.Infrastructure.Services.Security;
using Home.WebUI.Infrastructure.UriProvider;
using Home.WebUI.Infrastructure.Values;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;

var _Builder = WebApplication.CreateBuilder(args);

// Add services to the container.
_Builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options => options.DetailedErrors = true);

_Builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        // This cookie *is* the session. It arrives with the request that starts the circuit, so a
        // reload knows who you are before a single component renders — nothing is read out of the
        // browser and nothing can fail in a way that looks like being signed out.
        //
        // SameAsRequest rather than Always is deliberate — the tablet reaches this app over the
        // LAN, and a cookie the browser silently drops on plain HTTP would be worse than useless.
        options.LoginPath = AuthorisationUriProvider.GetLoginUri();
        options.Cookie.Name = "Home.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.IsEssential = true;

        // Matched to the refresh token's own life, and sliding, so a tablet in daily use is never
        // asked again. Anything shorter is a password prompt with a timer on it.
        options.ExpireTimeSpan = TimeSpan.FromDays(90);
        options.SlidingExpiration = true;
    });

_Builder.Services.AddAuthorization();
_Builder.Services.AddHttpContextAccessor();

// Hands the signed-in principal to every component through CascadingAuthenticationState. This is
// what replaced the custom AuthenticationStateProvider that had to reach into browser storage.
_Builder.Services.AddCascadingAuthenticationState();

// The key ring protects the stored OAuth token, so losing it signs every family member out.
// Naming the application pins the discriminator to that name instead of the content root path,
// which means moving or renaming the folder no longer invalidates the sessions on every tablet.
_ = _Builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(_Builder.Environment.ContentRootPath, "DataProtectionKeys")))
    .SetApplicationName("Home.WebUI")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(365));

var _ApiBaseUrlString = _Builder.Configuration["apiBaseUrl"];

if (string.IsNullOrWhiteSpace(_ApiBaseUrlString))
    throw new InvalidOperationException("API base URL is not configured.");

if (!Uri.TryCreate(_ApiBaseUrlString, UriKind.Absolute, out var _ApiBaseUrl))
    throw new InvalidOperationException("API base URL is malformed.");

_ = _Builder.Services.AddHttpClient(HttpClientValues.ApiClientName, options => options.BaseAddress = _ApiBaseUrl);

_ = _Builder.Services.AddScoped<IHomeHttpClient>(sp => new HomeHttpClient(
    sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientValues.ApiClientName),
    sp.GetRequiredService<IHouseholdSession>()));

// The token endpoint is reached from two places that must not depend on each other: the sign-in
// endpoint during a request, and the circuit's session when its access token runs out.
_ = _Builder.Services.AddScoped<IOAuthClient>(sp => new OAuthClient(
    sp.GetRequiredService<IConfiguration>(),
    sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientValues.ApiClientName)));

// Scoped so one circuit shares one access token and one refresh gate.
_Builder.Services.AddScoped<IHouseholdSession, HouseholdSession>();

// The BCL clock abstraction (.NET 8). Components read the time through this rather than
// DateTime.Now, which also keeps "now" consistent across a single render.
_Builder.Services.AddSingleton(TimeProvider.System);

// Live cross-device updates: the broker is the process-wide fan-out between circuits, and
// each circuit talks to it through a broadcaster that pins the caller's own household.
_Builder.Services.AddSingleton<IChangeBroker, ChangeBroker>();
_Builder.Services.AddScoped<IChangeBroadcaster, ChangeBroadcaster>();

// Singleton so the count survives the circuit: a new browser tab must not reset it.
_Builder.Services.AddSingleton<ILoginThrottle, LoginThrottle>();

// A tunnel or reverse proxy terminates TLS at its edge and hands this app plain HTTP, so
// without these headers UseHttpsRedirection would bounce a phone to the machine's own
// localhost address. Both lists are cleared because the proxy is not on a known network —
// which does mean these headers are trusted from any caller, so the app must only ever be
// reachable through the proxy, never directly from the internet.
_ = _Builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});


var _App = _Builder.Build();

// Configure the HTTP request pipeline.
if (!_App.Environment.IsDevelopment())
{
    _ = _App.UseExceptionHandler("/Error", createScopeForErrors: true);
    _ = _App.UseHsts();
}

// Before anything that reads the scheme or the caller's address — chiefly the redirect below.
_App.UseForwardedHeaders();

_App.UseStaticFiles();
_App.UseHttpsRedirection();
_App.UseRouting();

_App.UseAuthentication();
_App.UseAuthorization();

_App.UseWebSockets();
_App.UseAntiforgery();

_App.MapRecipeImageEndpoints();

_App.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

_App.Run();
