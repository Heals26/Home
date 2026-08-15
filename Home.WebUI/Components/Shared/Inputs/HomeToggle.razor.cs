using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Shared.Inputs;

public partial class HomeToggle
{

    #region Properties

    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Value { get; set; }
    [Parameter] public EventCallback<bool> ValueChanged { get; set; }

    #endregion Properties

    #region Methods

    private async Task ToggleAsync()
    {
        if (!this.Disabled)
            await this.ValueChanged.InvokeAsync(!this.Value);
    }

    private string GetClasses()
        => $"relative inline-flex h-8 w-14 items-center rounded-full border transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-lights focus:ring-offset-2 focus:ring-offset-ink-950 disabled:opacity-50 {(this.Value ? "bg-lights border-lights" : "bg-ink-800 border-ink-600")}";

    /// <summary>
    /// Both knob colours are theme tokens rather than a literal white, which vanished against the
    /// light theme's pale off-track. ink-50 and ink-950 swap with the theme, so the knob keeps its
    /// contrast against the dark track when off and the amber track when on, either way round.
    /// </summary>
    private string GetThumbClasses()
        => $"inline-block h-6 w-6 transform rounded-full shadow-lg ring-1 ring-ink-950/20 transition-transform duration-200 {(this.Value ? "translate-x-6 bg-ink-950" : "translate-x-1 bg-ink-50")}";

    #endregion Methods

}
