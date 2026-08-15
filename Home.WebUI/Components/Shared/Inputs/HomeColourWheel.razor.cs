using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Home.WebUI.Components.Shared.Inputs;

public partial class HomeColourWheel
{

    #region Records

    private sealed record Wedge(string Fill, string Path);

    #endregion Records

    #region Fields

    // The SVG is drawn and sized in the same units, so a pointer's offset inside it already is a
    // wheel coordinate and nothing has to be measured in the browser.
    private const double Centre = 112d;
    private const double Radius = 104d;
    private const int WedgeCount = 72;

    private const int FallbackMaxKelvin = 9000;
    private const int FallbackMinKelvin = 2500;

    private static readonly Wedge[] Wedges = BuildWedges();

    private readonly string m_GradientID = $"colour-wheel-{Guid.NewGuid():N}";

    private double m_Hue;
    private double m_Saturation;
    private bool m_IsDragging;
    private bool m_ShowWheel;

    #endregion Fields

    #region Properties

    /// <summary>
    /// 0.0 to 1.0, matching the bulb rather than the slider's percentage.
    /// </summary>
    [Parameter] public double Brightness { get; set; }

    [Parameter] public EventCallback<double> BrightnessChanged { get; set; }
    [Parameter] public EventCallback<double> BrightnessCommitted { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public EventCallback<(double Hue, double Saturation)> ColourChanged { get; set; }
    [Parameter] public EventCallback<(double Hue, double Saturation)> ColourCommitted { get; set; }
    [Parameter] public bool HasColour { get; set; }
    [Parameter] public bool HasVariableColourTemp { get; set; }
    [Parameter] public double Hue { get; set; }
    [Parameter] public int Kelvin { get; set; }
    [Parameter] public EventCallback<int> KelvinChanged { get; set; }
    [Parameter] public EventCallback<int> KelvinCommitted { get; set; }
    [Parameter] public int MaxKelvin { get; set; }
    [Parameter] public int MinKelvin { get; set; }
    [Parameter] public double Saturation { get; set; }

    #endregion Properties

    #region Lifecycle Methods

    // Mid-drag the finger is the truth; the rest of the time the bulb is.
    protected override void OnParametersSet()
    {
        if (this.m_IsDragging)
            return;

        this.m_Hue = this.Hue;
        this.m_Saturation = this.Saturation;
    }

    #endregion Lifecycle Methods

    #region Methods

    private double BrightnessPercent()
        => Math.Round(Math.Clamp(this.Brightness, 0d, 1d) * 100d);

    private async Task OnBrightnessPreview(double percent)
        => await this.BrightnessChanged.InvokeAsync(Math.Clamp(percent / 100d, 0d, 1d));

    private async Task OnBrightnessRelease(double percent)
        => await this.BrightnessCommitted.InvokeAsync(Math.Clamp(percent / 100d, 0d, 1d));

    private double KelvinFloor()
        => this.MinKelvin > 0 ? this.MinKelvin : FallbackMinKelvin;

    private double KelvinCeiling()
        => this.MaxKelvin > 0 ? this.MaxKelvin : FallbackMaxKelvin;

    // A bulb parked outside its own range would drag the slider off its track.
    private double KelvinValue()
        => Math.Clamp(this.Kelvin, this.KelvinFloor(), this.KelvinCeiling());

    private async Task OnKelvinPreview(double kelvin)
        => await this.KelvinChanged.InvokeAsync((int)Math.Round(kelvin));

    private async Task OnKelvinRelease(double kelvin)
        => await this.KelvinCommitted.InvokeAsync((int)Math.Round(kelvin));

    private void ToggleWheel()
        => this.m_ShowWheel = !this.m_ShowWheel;

    private async Task OnPointerDown(PointerEventArgs e)
    {
        this.m_IsDragging = true;

        await this.TrackPointerAsync(e);
    }

    private async Task OnPointerMove(PointerEventArgs e)
    {
        if (this.m_IsDragging)
            await this.TrackPointerAsync(e);
    }

    /// <summary>
    /// There is no pointer capture without JavaScript, so a finger that slides off the wheel
    /// commits where it left rather than stranding the drag.
    /// </summary>
    private async Task OnPointerRelease()
    {
        if (!this.m_IsDragging)
            return;

        this.m_IsDragging = false;

        await this.ColourCommitted.InvokeAsync((this.m_Hue, this.m_Saturation));
    }

    /// <summary>
    /// Angle is hue, distance from the centre is saturation. Only the preview moves — LIFX allows
    /// about 120 requests a minute, so the bulb is told once, on release.
    /// </summary>
    private async Task TrackPointerAsync(PointerEventArgs e)
    {
        var _X = e.OffsetX - Centre;
        var _Y = Centre - e.OffsetY;
        var _Degrees = Math.Atan2(_Y, _X) * 180d / Math.PI;

        this.m_Hue = _Degrees < 0d ? _Degrees + 360d : _Degrees;
        this.m_Saturation = Math.Clamp(Math.Sqrt((_X * _X) + (_Y * _Y)) / Radius, 0d, 1d);

        await this.ColourChanged.InvokeAsync((this.m_Hue, this.m_Saturation));
    }

    private string MarkerX()
        => Format(Centre + (this.m_Saturation * Radius * Math.Cos(this.m_Hue * Math.PI / 180d)));

    private string MarkerY()
        => Format(Centre - (this.m_Saturation * Radius * Math.Sin(this.m_Hue * Math.PI / 180d)));

    // Saturation shows as lightness so the centre reads white, the way the wheel itself is drawn.
    private string PreviewCss()
        => $"hsl({Format(this.m_Hue)}, 100%, {Format(100d - (this.m_Saturation * 45d))}%)";

    /// <summary>
    /// The wheel is a fixed set of pie slices, one hue each, with a white radial gradient over the
    /// top doing the saturation falloff — no canvas, no image, no library.
    /// </summary>
    private static Wedge[] BuildWedges()
    {
        const double _Step = 360d / WedgeCount;

        var _Wedges = new Wedge[WedgeCount];

        for (var _Index = 0; _Index < WedgeCount; _Index++)
        {
            var _From = _Index * _Step;

            // Slices overlap slightly so no hairline of the page shows between them.
            var (_StartX, _StartY) = PointOn(_From);
            var (_EndX, _EndY) = PointOn(_From + _Step + 0.4d);

            _Wedges[_Index] = new(
                $"hsl({Format(_From + (_Step / 2d))}, 100%, 50%)",
                $"M{Format(Centre)} {Format(Centre)} L{Format(_StartX)} {Format(_StartY)} "
                    + $"A{Format(Radius)} {Format(Radius)} 0 0 0 {Format(_EndX)} {Format(_EndY)} Z");
        }

        return _Wedges;
    }

    private static (double X, double Y) PointOn(double degrees)
        => (Centre + (Radius * Math.Cos(degrees * Math.PI / 180d)),
            Centre - (Radius * Math.Sin(degrees * Math.PI / 180d)));

    // SVG will not read "1,5" as a number, so the machine's own decimal separator stays out of it.
    private static string Format(double value)
        => value.ToString("0.##", CultureInfo.InvariantCulture);

    #endregion Methods

}
