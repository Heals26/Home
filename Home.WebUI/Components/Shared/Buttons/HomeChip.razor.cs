using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Shared.Buttons;

public partial class HomeChip
{

    #region Properties

    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public EventCallback OnClick { get; set; }
    [Parameter] public bool Selected { get; set; }

    #endregion Properties

    #region Methods

    private string GetClasses()
    {
        var _Base = "inline-flex shrink-0 items-center gap-2 rounded-full border px-4 min-h-[40px] text-sm font-medium transition-colors enabled:active:scale-95";

        var _State = this.Selected
            ? "border-ink-50 bg-ink-50 text-ink-950"
            : "border-ink-700 bg-ink-800 text-ink-300 enabled:hover:text-ink-50 enabled:hover:border-ink-600";

        return string.Join(' ', new[] { _Base, _State, this.Class }.Where(c => !string.IsNullOrEmpty(c)));
    }

    #endregion Methods

}
