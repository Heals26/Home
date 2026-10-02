using Home.WebUI.DataAccess.Support.CreateFeedback;
using Home.WebUI.DataAccess.Support.Models;
using Home.WebUI.Infrastructure.Services.Support;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

namespace Home.WebUI.Infrastructure.Support;

/// <summary>
/// Scoped per circuit and first resolved by the sheet the rail renders on every page, so it has
/// watched every page the tab has been on and can say which one feedback sent from Settings is
/// about.
/// </summary>
public class FeedbackLogic : IFeedbackLogic, IDisposable
{

    #region Fields

    private readonly IJSRuntime m_JSRuntime;
    private string? m_LastPageOutsideSettings;
    private readonly NavigationManager m_NavigationManager;

    #endregion Fields

    #region Constructors

    public FeedbackLogic(IJSRuntime jsRuntime, NavigationManager navigationManager)
    {
        this.m_JSRuntime = jsRuntime;
        this.m_NavigationManager = navigationManager;

        this.Remember(navigationManager.Uri);
        navigationManager.LocationChanged += this.OnLocationChanged;
    }

    #endregion Constructors

    #region Methods

    public async Task<CreateFeedbackWebAppRequest> BuildRequestAsync(FeedbackKind kind, string content, bool fromSettings, CancellationToken cancellationToken)
    {
        var _CurrentPage = this.ToPage(this.m_NavigationManager.Uri);
        var _ScreenSize = await this.GetScreenSizeAsync(cancellationToken);

        return new CreateFeedbackWebAppRequest()
        {
            Content = content.Trim(),
            Kind = kind,
            Page = fromSettings ? this.m_LastPageOutsideSettings ?? _CurrentPage : _CurrentPage,
            ScreenHeight = _ScreenSize?.Height,
            ScreenWidth = _ScreenSize?.Width
        };
    }

    public void Dispose()
    {
        this.m_NavigationManager.LocationChanged -= this.OnLocationChanged;

        GC.SuppressFinalize(this);
    }

    private async Task<ScreenSize?> GetScreenSizeAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await this.m_JSRuntime.InvokeAsync<ScreenSize>("homeScreen.size", cancellationToken);
        }
        catch (Exception _Exception) when (_Exception is JSException or InvalidOperationException or TaskCanceledException or JSDisconnectedException)
        {
            // The browser would not say, and feedback is worth more sent without a size than refused.
            return null;
        }
    }

    private static bool IsSettings(string page)
        => page.Split('?', '#')[0].TrimEnd('/').Equals("/settings", StringComparison.OrdinalIgnoreCase);

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        => this.Remember(e.Location);

    private void Remember(string uri)
    {
        var _Page = this.ToPage(uri);

        if (!IsSettings(_Page))
            this.m_LastPageOutsideSettings = _Page;
    }

    private string ToPage(string uri)
        => $"/{this.m_NavigationManager.ToBaseRelativePath(uri)}";

    #endregion Methods

}
