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

    /// <summary>
    /// Sentence case on prose so a phone keyboard capitalises "onion" the way a person would,
    /// and off everywhere else — an address or a token must arrive exactly as typed.
    /// </summary>
    private string GetAutoCapitalise()
        => this.IsProse() ? "sentences" : "off";

    /// <summary>
    /// A field with no explicit name gets its own unguessable one. Browsers decide whether to
    /// offer contacts by pattern-matching the name, id and label — a field labelled "Name" gets
    /// offered a contact card whatever <c>autocomplete="off"</c> says — and nothing matches a
    /// GUID. Fields that genuinely want autofill (username, password) pass a real name.
    /// </summary>
    private string GetName()
        => string.IsNullOrEmpty(this.Name) ? this.m_InputID : this.Name;

    /// <summary>
    /// Prose gets the red squiggle; names, addresses and numbers don't. Browsers guess this
    /// inconsistently, so it is said outright: spellcheck belongs on plain text the user is
    /// composing, never on identity fields or anything typed on a numeric keyboard.
    /// </summary>
    private string GetSpellCheck()
        => this.IsProse() ? "true" : "false";

    /// <summary>
    /// Plain text the user is composing, as opposed to an address, a number or a credential.
    /// </summary>
    private bool IsProse()
        => this.Type == "text"
            && this.InputMode == null
            && (this.AutoComplete == null || this.AutoComplete == "off");

    #endregion Methods

}
