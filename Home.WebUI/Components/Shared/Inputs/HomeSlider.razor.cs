using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Shared.Inputs;

public partial class HomeSlider
{

    #region Properties

    [Parameter] public string? Class { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? Label { get; set; }
    [Parameter] public double Max { get; set; } = 100;
    [Parameter] public double Min { get; set; } = 0;
    [Parameter] public double Step { get; set; } = 1;
    [Parameter] public string Unit { get; set; } = string.Empty;
    [Parameter] public double Value { get; set; }
    [Parameter] public EventCallback<double> ValueChanged { get; set; }
    [Parameter] public EventCallback<double> ValueCommitted { get; set; }

    #endregion Properties

    #region Methods

    private async Task OnInputChanged(ChangeEventArgs e)
    {
        if (double.TryParse(e.Value?.ToString(), out var _Value))
            await this.ValueChanged.InvokeAsync(_Value);
    }

    private async Task OnCommitted(ChangeEventArgs e)
    {
        if (this.ValueCommitted.HasDelegate && double.TryParse(e.Value?.ToString(), out var _Value))
            await this.ValueCommitted.InvokeAsync(_Value);
    }

    #endregion Methods

}
