using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Shared.Cards;

public partial class HomeCard
{

    #region Properties

    [Parameter] public RenderFragment? ActionsContent { get; set; }

    /// <summary>
    /// Lets content escape the card's bounds. Off by default, because the rounded corners work by
    /// clipping and a full-bleed row at the bottom would otherwise square them off.
    /// <para>
    /// Turn it on for a card holding something that floats, such as a suggestion list under a text
    /// box: clipped, that list is cut off at the card's edge, and on a short card there is barely
    /// any card to be cut off inside.
    /// </para>
    /// </summary>
    [Parameter] public bool AllowOverflow { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public bool Padded { get; set; } = true;
    [Parameter] public string? Subtitle { get; set; }
    [Parameter] public string? Title { get; set; }
    [Parameter] public RenderFragment? TitleContent { get; set; }

    #endregion Properties

    #region Methods

    private string GetClasses()
        => $"bg-ink-900 rounded-2xl border border-ink-800 {(this.AllowOverflow ? "overflow-visible" : "overflow-hidden")} {this.Class}";

    #endregion Methods

}
