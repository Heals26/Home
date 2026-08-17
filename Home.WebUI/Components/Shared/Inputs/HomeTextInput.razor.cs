using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Home.WebUI.Components.Shared.Inputs;

public partial class HomeTextInput
{

    #region Fields

    private readonly string m_InputID = $"home-input-{Guid.NewGuid():N}";
    private ElementReference m_Input;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Anything else the caller puts on the tag, which is how a combobox gets its aria attributes
    /// without every one of them becoming a parameter here.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    /// <summary>
    /// The HTML autocomplete token, e.g. "username", "email", "given-name", or "off".
    /// </summary>
    [Parameter] public string? AutoComplete { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? Error { get; set; }
    private string ErrorID => $"{this.m_InputID}-error";
    /// <summary>
    /// The virtual keyboard hint, e.g. "numeric" or "decimal".
    /// </summary>
    [Parameter] public string? InputMode { get; set; }
    [Parameter] public string? Label { get; set; }
    /// <summary>
    /// The HTML name attribute — browsers use it alongside autocomplete to match saved values.
    /// </summary>
    [Parameter] public string? Name { get; set; }
    [Parameter] public EventCallback OnBlur { get; set; }
    [Parameter] public EventCallback OnFocus { get; set; }
    [Parameter] public EventCallback<KeyboardEventArgs> OnKeyDown { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public string Type { get; set; } = "text";
    [Parameter] public string Value { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> ValueChanged { get; set; }

    #endregion Properties

    #region Methods

    /// <summary>
    /// Lets a caller put the cursor back where it was, so a screen built around typing one thing
    /// after another does not need a tap between each one.
    /// </summary>
    public ValueTask FocusAsync()
        => this.m_Input.FocusAsync();

    private async Task OnInputChanged(ChangeEventArgs e)
        => await this.ValueChanged.InvokeAsync(e.Value?.ToString() ?? string.Empty);

    private string GetInputClasses()
    {
        var _Base = "w-full bg-ink-800 border rounded-lg px-4 py-3 text-sm text-ink-50 placeholder-ink-500 transition-colors focus:outline-none focus:ring-2 focus:ring-ink-300 focus:border-transparent disabled:opacity-50 disabled:cursor-not-allowed min-h-[48px]";
        var _Border = string.IsNullOrEmpty(this.Error) ? "border-ink-700" : "border-red-500";
        return $"{_Base} {_Border}";
    }

    #endregion Methods

}
