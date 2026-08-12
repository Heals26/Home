using System.Globalization;

namespace Home.WebUI.Infrastructure.Values;

/// <summary>
/// One tappable colour on the Lights page. Kelvin presets carry a saturation of zero, because the
/// API drives saturation to zero whenever a white temperature is set.
/// </summary>
public record ColourPreset(string Name, double Hue, double Saturation, int Kelvin, string Css)
{

    #region Constants

    /// <summary>
    /// Ordered warm-to-cool whites first, then the spectrum. Eleven fits two rows on a phone and
    /// one on a tablet.
    /// </summary>
    public static readonly ColourPreset[] All =
    [
        new("Warm White", 0, 0, 2700, "#ffd9a0"),
        new("Neutral White", 0, 0, 4000, "#ffe9cc"),
        new("Cool White", 0, 0, 6500, "#e8f1ff"),
        new("Red", 0, 1, 0, "hsl(0, 90%, 55%)"),
        new("Orange", 30, 1, 0, "hsl(30, 90%, 55%)"),
        new("Yellow", 55, 1, 0, "hsl(55, 90%, 55%)"),
        new("Green", 120, 1, 0, "hsl(120, 70%, 45%)"),
        new("Teal", 175, 1, 0, "hsl(175, 80%, 45%)"),
        new("Blue", 220, 1, 0, "hsl(220, 90%, 55%)"),
        new("Purple", 280, 1, 0, "hsl(280, 70%, 55%)"),
        new("Pink", 320, 1, 0, "hsl(320, 85%, 60%)")
    ];

    #endregion Constants

    #region Properties

    /// <summary>
    /// True when this preset sets a white temperature rather than a hue.
    /// </summary>
    public bool IsWhite => this.Kelvin > 0;

    #endregion Properties

    #region Methods

    /// <summary>
    /// Approximates a bulb's current colour for a status dot. Saturation at zero means the bulb is
    /// showing white, so fall back to a warm tone rather than rendering it grey.
    /// </summary>
    public static string SwatchFor(bool isConnected, bool isOn, double brightness, double hue, double saturation)
    {
        if (!isConnected || !isOn)
            return "#3f3f46";

        var _Lightness = Round(35 + (Math.Clamp(brightness, 0d, 1d) * 35d));

        return saturation <= 0.01
            ? $"hsl(40, 55%, {_Lightness}%)"
            : $"hsl({Round(hue)}, {Round(Math.Clamp(saturation, 0d, 1d) * 100d)}%, {_Lightness}%)";
    }

    private static string Round(double value)
        => value.ToString("0", CultureInfo.InvariantCulture);

    #endregion Methods

}
