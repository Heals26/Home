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

    private void GoBack()
        => this.NavigationManager.NavigateTo(this.BackHref);

    #endregion Methods

}
