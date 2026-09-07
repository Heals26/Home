using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Shared.Inputs;

public partial class HomeCheckbox
{

    #region Properties

    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Value { get; set; }
    [Parameter] public EventCallback<bool> ValueChanged { get; set; }

    #endregion Properties

    #region Methods

    private async Task OnCheckedChangedAsync(ChangeEventArgs e)
        => await this.ValueChanged.InvokeAsync(e.Value is true);

    private string GetLabelClasses()
        => string.Join(' ', new[]
        {
            "flex items-center gap-3 px-4 py-3 min-h-[48px] cursor-pointer transition-colors",
            this.Disabled ? "cursor-not-allowed opacity-50" : "hover:bg-ink-800/60",
            this.Class
        }.Where(c => !string.IsNullOrEmpty(c)));

    #endregion Methods

}
