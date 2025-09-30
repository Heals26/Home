using Home.WebUI.Components;
using Home.WebUI.Infrastructure.HttpClients;
using Home.WebUI.Infrastructure.Services.HttpClients;
using MudBlazor.Services;

var _Builder = WebApplication.CreateBuilder(args);

// Add services to the container.
_Builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

_Builder.Services.AddHttpClient<IHomeHttpClient, HomeHttpClient>(options =>
{
    var _BaseUrlString = _Builder.Configuration["apiBaseUrl"];

    if (_BaseUrlString == null)
        throw new InvalidOperationException("API base URL is not configured.");
    else if (!Uri.TryCreate(_BaseUrlString, UriKind.Absolute, out var _BaseUrl))
        throw new InvalidOperationException("API base URL is malformed.");

    options.BaseAddress = new(_BaseUrlString);
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

_App.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

_App.Run();
