using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Shared.Buttons;

public partial class HomeListRow
{

    #region Properties

    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public bool Disabled { get; set; }
    /// <summary>
    /// The "add another one" row at the foot of a list, drawn as an outline rather than a fill so
    /// it reads as a slot rather than an entry.
    /// </summary>
    [Parameter] public bool Dashed { get; set; }
    [Parameter] public bool Inset { get; set; }
    [Parameter] public bool KeepFocusOnMouseDown { get; set; }
    [Parameter] public EventCallback OnClick { get; set; }

    #endregion Properties

    #region Methods

    private string GetClasses()
    {
        var _Base = "w-full text-left transition-colors";
        var _Padding = this.Inset ? "px-5 py-3 min-h-[48px]" : "px-4 py-3 min-h-[48px]";

        var _Look = this.Dashed
            ? "flex items-center gap-3 rounded-lg border border-dashed border-ink-700 enabled:hover:border-ink-500 enabled:hover:bg-ink-800"
            : "enabled:hover:bg-ink-800";

        return string.Join(' ', new[] { _Base, _Padding, _Look, this.Class }.Where(c => !string.IsNullOrEmpty(c)));
    }

    #endregion Methods

}
