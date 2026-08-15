using Microsoft.AspNetCore.Components;

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
    [Parameter] public bool Loading { get; set; }
    [Parameter] public EventCallback OnClick { get; set; }
    [Parameter] public string Size { get; set; } = "md";
    [Parameter] public string Type { get; set; } = "button";
    [Parameter] public string Variant { get; set; } = "primary";

    #endregion Properties

    #region Methods

    private string GetClasses()
    {
        var _Base = "relative inline-flex items-center justify-center font-medium rounded-lg transition-all duration-150 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-offset-ink-950 disabled:opacity-50 disabled:cursor-not-allowed active:scale-95";

        var _Size = this.Size switch
        {
            "sm" => "px-3 py-2 text-sm min-h-[36px]",
            "lg" => "px-6 py-4 text-base min-h-[56px]",
            _    => "px-4 py-3 text-sm min-h-[48px]"
        };

        var _Variant = this.Variant switch
        {
            "secondary" => "bg-ink-800 text-ink-50 hover:bg-ink-700 focus:ring-ink-600 border border-ink-700",
            "ghost"     => "bg-transparent text-ink-400 hover:text-ink-50 hover:bg-ink-800 focus:ring-ink-700",
            "danger"    => "bg-red-600 text-white hover:bg-red-500 focus:ring-red-500",
            _           => "bg-ink-50 text-ink-950 hover:bg-white focus:ring-ink-300"
        };

        return $"{_Base} {_Size} {_Variant} {this.Class}";
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

    #endregion Methods

}
