using Microsoft.AspNetCore.Components;
using System.Text.RegularExpressions;

namespace Home.WebUI.Components.Shared.Inputs;

public partial class HomeColourPicker
{

    #region Fields

    private static readonly Regex ColourPattern = new("^#[0-9a-fA-F]{6}$", RegexOptions.Compiled);

    /// <summary>
    /// Legible against the dark surfaces the app lives on, and distinguishable from each other
    /// at arm's length on a kitchen tablet.
    /// </summary>
    private static readonly string[] Swatches =
    [
        "#f87171",
        "#fb923c",
        "#fbbf24",
        "#a3b18a",
        "#34d399",
        "#7dd3fc",
        "#818cf8",
        "#c084fc",
        "#f472b6",
        "#a8a29e"
    ];

    private readonly string m_InputID = $"home-colour-{Guid.NewGuid():N}";

    #endregion Fields

    #region Properties

    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Label { get; set; }
    [Parameter] public string Value { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> ValueChanged { get; set; }

    #endregion Properties

    #region Methods

    /// <summary>
    /// The one place a stored colour is trusted — anything that is not #RRGGBB never reaches
    /// an inline style.
    /// </summary>
    public static bool IsValidColour(string? colour)
        => !string.IsNullOrWhiteSpace(colour) && ColourPattern.IsMatch(colour);

    private async Task OnColourChangedAsync(ChangeEventArgs e)
    {
        var _Colour = e.Value?.ToString() ?? string.Empty;

        if (IsValidColour(_Colour))
            await this.SelectAsync(_Colour);
    }

    private async Task SelectAsync(string colour)
        => await this.ValueChanged.InvokeAsync(colour);

    private bool IsSelected(string colour)
        => string.Equals(this.Value, colour, StringComparison.OrdinalIgnoreCase);

    private string GetSwatchClasses(string colour)
    {
        var _Base = "h-12 w-12 shrink-0 rounded-lg flex items-center justify-center transition active:scale-95";
        var _Ring = this.IsSelected(colour) ? "ring-2 ring-offset-2 ring-offset-ink-900 ring-ink-50" : "";

        return $"{_Base} {_Ring}";
    }

    #endregion Methods

}
