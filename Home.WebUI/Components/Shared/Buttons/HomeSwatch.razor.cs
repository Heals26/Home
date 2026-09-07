using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Shared.Buttons;

public partial class HomeSwatch
{

    #region Properties

    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public EventCallback OnClick { get; set; }
    [Parameter] public bool Selected { get; set; }
    [Parameter] public string Size { get; set; } = "md";
    [Parameter] public string? Style { get; set; }

    #endregion Properties

    #region Methods

    private string GetClasses()
    {
        var _Size = this.Size == "lg" ? "h-12 w-12" : "h-11 w-11";

        var _State = this.Selected
            ? "border-lights ring-2 ring-lights/40"
            : "border-ink-700 enabled:hover:border-ink-500";

        return string.Join(' ', new[] { "shrink-0 rounded-lg border transition-all enabled:active:scale-95", _Size, _State, this.Class }
            .Where(c => !string.IsNullOrEmpty(c)));
    }

    #endregion Methods

}
