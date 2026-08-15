using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Shared.Navigation;

public partial class HomeTopBar
{

    #region Properties

    [Parameter] public RenderFragment? ActionsContent { get; set; }
    [Parameter] public string BackHref { get; set; } = "/";
    [Parameter] public string? Eyebrow { get; set; }
    [Parameter] public string EyebrowClass { get; set; } = "text-ink-500";
    [Parameter] public bool ShowBack { get; set; }
    [Parameter] public string? Subtitle { get; set; }
    [Parameter] public string Title { get; set; } = string.Empty;

    #endregion Properties

    #region Methods

    // Always a real destination, never browser history — history can be empty on a deep link,
    // and a blank BackHref would throw.
    private void GoBack()
        => this.NavigationManager.NavigateTo(string.IsNullOrWhiteSpace(this.BackHref) ? "/" : this.BackHref);

    #endregion Methods

}
