using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Home.WebUI.Components.Shared.Buttons;

public partial class HomeButton
{

    #region Fields

    private bool m_ShowDisabledReason;

    #endregion Fields

    #region Properties

    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public bool Disabled { get; set; }
    /// <summary>
    /// Why the button cannot be used. With one set, a disabled button still looks and behaves as
    /// tappable and shows this instead of doing nothing.
    /// </summary>
    [Parameter] public string? DisabledReason { get; set; }
    /// <summary>
    /// A button whose whole content is an icon, which wants to be square rather than a short wide
    /// pill. The tap target is the same height either way, so the size still says how big.
    /// </summary>
    [Parameter] public bool IconOnly { get; set; }
    /// <summary>
    /// Stops the press taking focus off whatever has it, which is how a suggestion under a text box
    /// gets tapped without the box blurring and closing the list first.
    /// </summary>
    [Parameter] public bool KeepFocusOnMouseDown { get; set; }
    [Parameter] public bool Loading { get; set; }
    [Parameter] public EventCallback OnClick { get; set; }
    [Parameter] public EventCallback<DragEventArgs> OnDragStart { get; set; }
    [Parameter] public bool PreventDefault { get; set; }
    [Parameter] public string Size { get; set; } = "md";
    /// <summary>
    /// For a button sitting inside something else tappable, such as the tick on a card that opens.
    /// </summary>
    [Parameter] public bool StopPropagation { get; set; }
    [Parameter] public string Type { get; set; } = "button";
    [Parameter] public string Variant { get; set; } = "primary";

    #endregion Properties

    #region Methods

    private string GetClasses()
    {
        // The only <button> in the app, so "bare" has to exist: a caller that wants a row, a chip
        // or a swatch takes the focus ring and the disabled handling and draws the rest itself.
        var _IsBare = this.Variant == "bare";

        // enabled: on the hover and press states, because a button that is genuinely off should not
        // light up under a cursor. One held open by a DisabledReason is still enabled in the DOM,
        // so it keeps reacting, which is the point of it.
        // relative is here only to anchor the DisabledReason bubble, and Tailwind emits it after
        // absolute, so it would win on order and strand a button the caller meant to float.
        var _Position = this.PositionsItself() ? string.Empty : "relative ";

        var _Base = _IsBare
            ? $"{_Position}transition-all duration-150 focus:outline-none focus:ring-2 focus:ring-inset focus:ring-ink-600 disabled:opacity-50 disabled:cursor-not-allowed"
            : $"{_Position}inline-flex items-center justify-center font-medium rounded-lg transition-all duration-150 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-offset-ink-950 disabled:opacity-50 disabled:cursor-not-allowed enabled:active:scale-95";

        var _Size = (this.IconOnly, this.Size) switch
        {
            (_, "none")   => string.Empty,
            (true, "xs")  => "h-8 w-8 shrink-0 text-xs",
            (true, "sm")  => "h-11 w-11 shrink-0 text-sm",
            (true, "lg")  => "h-14 w-14 shrink-0 text-base",
            (true, _)     => "h-12 w-12 shrink-0 text-sm",
            (false, "sm") => "px-3 py-2 text-sm min-h-[36px]",
            (false, "lg") => "px-6 py-4 text-base min-h-[56px]",
            _             => "px-4 py-3 text-sm min-h-[48px]"
        };

        var _Variant = this.Variant switch
        {
            "bare"      => string.Empty,
            "secondary" => "bg-ink-800 text-ink-50 enabled:hover:bg-ink-700 focus:ring-ink-600 border border-ink-700",
            "ghost"     => "bg-transparent text-ink-400 enabled:hover:text-ink-50 enabled:hover:bg-ink-800 focus:ring-ink-700",
            "link"      => "bg-transparent text-ink-300 underline underline-offset-4 enabled:hover:text-ink-50 focus:ring-ink-700",
            "danger"    => "bg-red-600 text-white enabled:hover:bg-red-500 focus:ring-red-500",
            _           => "bg-ink-50 text-ink-950 enabled:hover:bg-white focus:ring-ink-300"
        };

        return string.Join(' ', new[] { _Base, _Size, _Variant, this.Class }.Where(c => !string.IsNullOrEmpty(c)));
    }

    private async Task HandleClickAsync()
    {
        if (this.HasDisabledReason())
        {
            this.m_ShowDisabledReason = true;
            return;
        }

        this.m_ShowDisabledReason = false;

        if (this.OnClick.HasDelegate)
            await this.OnClick.InvokeAsync();
    }

    private bool HasDisabledReason()
        => this.Disabled && !string.IsNullOrWhiteSpace(this.DisabledReason);

    private bool IsHardDisabled()
        => this.Disabled && !this.HasDisabledReason();

    private bool PositionsItself()
        => this.Class != null
            && this.Class.Split(' ').Any(c => c is "absolute" or "fixed" or "sticky" or "static");

    #endregion Methods

}
