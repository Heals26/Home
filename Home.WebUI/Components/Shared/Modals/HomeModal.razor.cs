using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Shared.Modals;

public partial class HomeModal
{

    #region Properties

    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public RenderFragment? FooterContent { get; set; }
    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public bool Visible { get; set; }
    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }

    #endregion Properties

    #region Methods

    private async Task CloseAsync()
        => await this.VisibleChanged.InvokeAsync(false);

    #endregion Methods

}
