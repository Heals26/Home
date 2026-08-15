using Home.WebUI.DataAccess.Lights.Models;
using Home.WebUI.Infrastructure.Values;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Lights;

public partial class LightControlCard
{

    #region Properties

    [Parameter] public bool EditMode { get; set; }
    [Parameter, EditorRequired] public long GroupID { get; set; }
    [Parameter] public IReadOnlyList<LightGroupDto> Groups { get; set; } = [];
    [Parameter, EditorRequired] public LightDto Light { get; set; } = default!;
    [Parameter] public EventCallback<double> OnBrightnessCommitted { get; set; }
    [Parameter] public EventCallback<(double Hue, double Saturation)> OnColourCommitted { get; set; }
    [Parameter] public EventCallback<long> OnGroupChanged { get; set; }
    [Parameter] public EventCallback<int> OnKelvinCommitted { get; set; }
    [Parameter] public EventCallback<bool> OnPowerChanged { get; set; }
    [Parameter] public EventCallback<ColourPreset> OnPresetSelected { get; set; }

    #endregion Properties

    #region Methods

    /// <summary>
    /// Whites are filtered to the bulb's own kelvin range, so a preset it cannot reach is never
    /// shown. A bulb reporting no range at all is assumed to manage the usual 2500-9000.
    /// </summary>
    private IEnumerable<ColourPreset> AvailablePresets()
    {
        var _Min = this.Light.MinKelvin > 0 ? this.Light.MinKelvin : 2500;
        var _Max = this.Light.MaxKelvin > 0 ? this.Light.MaxKelvin : 9000;

        foreach (var _Preset in ColourPreset.All)
        {
            if (_Preset.IsWhite)
            {
                if (this.Light.HasVariableColourTemp && _Preset.Kelvin >= _Min && _Preset.Kelvin <= _Max)
                    yield return _Preset;
            }
            else if (this.Light.HasColour)
            {
                yield return _Preset;
            }
        }
    }

    // Marks the swatch the bulb is currently sitting on, so the card reflects reality.
    private bool IsCurrent(ColourPreset preset)
        => preset.IsWhite
            ? this.Light.Saturation <= 0.01 && Math.Abs(this.Light.Kelvin - preset.Kelvin) < 250
            : this.Light.Saturation > 0.01 && Math.Abs(this.Light.Hue - preset.Hue) < 12;

    private async Task OnGroupSelected(ChangeEventArgs e)
    {
        if (long.TryParse(e.Value?.ToString(), out var _GroupID) && _GroupID != this.GroupID)
            await this.OnGroupChanged.InvokeAsync(_GroupID);
    }

    private static int Percent(double value)
        => (int)Math.Round(Math.Clamp(value, 0d, 1d) * 100d);

    // The wheel drags against the card's own copy so the swatch tracks the finger.
    private void PreviewColour((double Hue, double Saturation) colour)
    {
        this.Light.Hue = colour.Hue;
        this.Light.Saturation = colour.Saturation;
    }

    private string StatusText()
        => !this.Light.IsConnected ? "Offline"
            : this.Light.IsOn ? $"On · {Percent(this.Light.Brightness)}%"
            : "Off";

    private string? ZoneBadge()
        => this.Light.HasMatrix ? "Matrix"
            : this.Light.HasMultizone ? "Strip"
            : null;

    #endregion Methods

}
