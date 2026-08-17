using Home.WebUI.Components;
using Microsoft.AspNetCore.DataProtection;
using Home.WebUI.Infrastructure.ChangeNotifications;
using Home.WebUI.Infrastructure.HttpClients;
using Home.WebUI.Infrastructure.Security;
using Home.WebUI.Infrastructure.Services.ChangeNotifications;
using Home.WebUI.Infrastructure.Services.HttpClients;
using Home.WebUI.Infrastructure.Services.Security;
using Home.WebUI.Infrastructure.UriProvider;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
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
        // Nothing signs in through this cookie. AuthorisationService is a custom
        // AuthenticationStateProvider whose state comes from a token in ProtectedLocalStorage;
        // the scheme is registered only so [Authorize] has a default and LoginPath resolves.
        // SameAsRequest rather than Always is deliberate — the tablet reaches this app over the
        // LAN, and a cookie the browser silently drops on plain HTTP would be worse than useless.
        options.LoginPath = AuthorisationUriProvider.GetLoginUri();
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

_Builder.Services.AddAuthorization();
_Builder.Services.AddHttpContextAccessor();

// The key ring protects the stored OAuth token, so losing it signs every family member out.
// Naming the application pins the discriminator to that name instead of the content root path,
// which means moving or renaming the folder no longer invalidates the sessions on every tablet.
_ = _Builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(_Builder.Environment.ContentRootPath, "DataProtectionKeys")))
    .SetApplicationName("Home.WebUI")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(365));

const string _ApiClientName = "HomeApi";

var _ApiBaseUrlString = _Builder.Configuration["apiBaseUrl"];

if (string.IsNullOrWhiteSpace(_ApiBaseUrlString))
    throw new InvalidOperationException("API base URL is not configured.");

if (!Uri.TryCreate(_ApiBaseUrlString, UriKind.Absolute, out var _ApiBaseUrl))
    throw new InvalidOperationException("API base URL is malformed.");

_ = _Builder.Services.AddHttpClient(_ApiClientName, options => options.BaseAddress = _ApiBaseUrl);

// Scoped, not the transient a typed client would give: HomeHttpClient serialises token refreshes
// through an instance semaphore, and that only holds if every component in a circuit shares one
// instance. AuthorisationService resolves the concrete type for its refresh at startup.
_ = _Builder.Services.AddScoped(sp => new HomeHttpClient(
    sp.GetRequiredService<IAuthorisationService>(),
    sp.GetRequiredService<IConfiguration>(),
    sp.GetRequiredService<IHttpClientFactory>().CreateClient(_ApiClientName),
    sp.GetRequiredService<ILoginThrottle>()));
_ = _Builder.Services.AddScoped<IHomeHttpClient>(sp => sp.GetRequiredService<HomeHttpClient>());
// The BCL clock abstraction (.NET 8). Components read the time through this rather than
// DateTime.Now, which also keeps "now" consistent across a single render.
_Builder.Services.AddSingleton(TimeProvider.System);
_Builder.Services.AddScoped<AuthorisationService>();
_Builder.Services.AddScoped<IAuthorisationService>(sp => sp.GetRequiredService<AuthorisationService>());
_Builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AuthorisationService>());

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

_App.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

_App.Run();
