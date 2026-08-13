using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Shared.Cards;

public partial class HomeCard
{

    #region Properties

    [Parameter] public RenderFragment? ActionsContent { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public bool Padded { get; set; } = true;
    [Parameter] public string? Subtitle { get; set; }
    [Parameter] public string? Title { get; set; }
    [Parameter] public RenderFragment? TitleContent { get; set; }

    #endregion Properties

    #region Methods

    private string GetClasses()
        => $"bg-ink-900 rounded-2xl border border-ink-800 overflow-hidden {this.Class}";

    #endregion Methods

}
