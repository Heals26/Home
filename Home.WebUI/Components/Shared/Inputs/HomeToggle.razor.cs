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
        => $"relative inline-flex h-8 w-14 items-center rounded-full transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-lights focus:ring-offset-2 focus:ring-offset-ink-950 disabled:opacity-50 {(this.Value ? "bg-lights" : "bg-ink-700")}";

    private string GetThumbClasses()
        => $"inline-block h-6 w-6 transform rounded-full shadow-lg transition-transform duration-200 {(this.Value ? "translate-x-7 bg-ink-950" : "translate-x-1 bg-white")}";

    #endregion Methods

}
