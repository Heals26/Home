using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Shared.Buttons;

public partial class HomeCardAction
{

    #region Properties

    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public bool Disabled { get; set; }
    /// <summary>
    /// Off for the first cell in the strip, on for the rest, so the rule falls between cells rather
    /// than against the card edge.
    /// </summary>
    [Parameter] public bool Divided { get; set; } = true;
    [Parameter] public EventCallback OnClick { get; set; }

    #endregion Properties

    #region Methods

    private string GetClasses()
    {
        var _Base = "flex-1 min-h-[48px] flex items-center justify-center text-ink-300 enabled:hover:bg-ink-700 enabled:active:scale-95 transition disabled:opacity-25";
        var _Divider = this.Divided ? "border-l border-ink-700" : string.Empty;

        return string.Join(' ', new[] { _Base, _Divider, this.Class }.Where(c => !string.IsNullOrEmpty(c)));
    }

    #endregion Methods

}
