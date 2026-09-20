using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Home.WebUI.Components.Shared.Inputs;

public partial class HomeTextInput
{

    #region Fields

    private readonly string m_InputID = $"home-input-{Guid.NewGuid():N}";
    private ElementReference m_Input;

    /// <summary>
    /// What the box is rendered with, which is not always what <see cref="Value"/> says.
    /// </summary>
    private string m_Shown = string.Empty;

    /// <summary>
    /// What the browser last said the box holds, or null when the caller last set it.
    /// </summary>
    private string? m_Typed;

    /// <summary>
    /// Counts the times the caller has written to the box. It keys the element, because two writes
    /// can carry the same text and the second one still has to reach a box the browser has changed
    /// since, which an unchanged attribute never would.
    /// </summary>
    private int m_Written;

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
    [Parameter] public string? Hint { get; set; }
    private string HintID => $"{this.m_InputID}-hint";
    /// <summary>
    /// The virtual keyboard hint, e.g. "numeric" or "decimal".
    /// </summary>
    [Parameter] public string? InputMode { get; set; }
    [Parameter] public string? Label { get; set; }
    /// <summary>
    /// The HTML name attribute. Browsers use it alongside autocomplete to match saved values.
    /// </summary>
    [Parameter] public string? Name { get; set; }
    [Parameter] public EventCallback OnBlur { get; set; }
    /// <summary>
    /// Fires when the field is left, not on every keystroke, for a value whose change costs a
    /// round trip. ValueChanged still fires as you type.
    /// </summary>
    [Parameter] public EventCallback<string> OnChange { get; set; }
    [Parameter] public EventCallback OnFocus { get; set; }
    [Parameter] public EventCallback<KeyboardEventArgs> OnKeyDown { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public string Type { get; set; } = "text";
    [Parameter] public string Value { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> ValueChanged { get; set; }

    #endregion Properties

    #region Lifecycle Methods

    /// <summary>
    /// The browser owns the box while someone is typing in it. Every keystroke here is a round trip,
    /// and rendering the value back into the box means the answer to one keystroke can land after the
    /// next two have been typed and put the box back as it was, which is how a shopping list ate
    /// letters. So the caller's value only reaches the box when it says something other than what was
    /// typed, which is a caller setting the text rather than echoing it.
    /// </summary>
    protected override void OnParametersSet()
    {
        if (this.m_Typed != null && this.Value == this.m_Typed)
            return;

        if (this.m_Typed != null)
            this.m_Written++;

        this.m_Shown = this.Value;
        this.m_Typed = null;
    }

    #endregion Lifecycle Methods

    #region Methods

    /// <summary>
    /// Lets a caller put the cursor back where it was, so a screen built around typing one thing
    /// after another does not need a tap between each one.
    /// </summary>
    public ValueTask FocusAsync()
        => this.m_Input.FocusAsync();

    private async Task OnInputChanged(ChangeEventArgs e)
    {
        this.m_Typed = e.Value?.ToString() ?? string.Empty;

        await this.ValueChanged.InvokeAsync(this.m_Typed);
    }

    private async Task OnChanged(ChangeEventArgs e)
        => await this.OnChange.InvokeAsync(e.Value?.ToString() ?? string.Empty);

    private string? GetDescribedBy()
    {
        List<string> _IDs = [];

        if (!string.IsNullOrEmpty(this.Hint))
            _IDs.Add(this.HintID);

        if (!string.IsNullOrEmpty(this.Error))
            _IDs.Add(this.ErrorID);

        return _IDs.Count == 0 ? null : string.Join(' ', _IDs);
    }

    private string GetInputClasses()
    {
        var _Base = "w-full bg-ink-800 border rounded-lg px-4 py-3 text-sm text-ink-50 placeholder-ink-500 transition-colors focus:outline-none focus:ring-2 focus:ring-ink-300 focus:border-transparent disabled:opacity-50 disabled:cursor-not-allowed min-h-[48px]";
        var _Border = string.IsNullOrEmpty(this.Error) ? "border-ink-700" : "border-red-500";
        return $"{_Base} {_Border}";
    }

    /// <summary>
    /// Sentence case on prose so a phone keyboard capitalises "onion" the way a person would,
    /// and off everywhere else, because an address or a token must arrive exactly as typed.
    /// </summary>
    private string GetAutoCapitalise()
        => this.IsProse() ? "sentences" : "off";

    /// <summary>
    /// A field with no explicit name gets its own unguessable one. Browsers decide whether to
    /// offer contacts by pattern-matching the name, id and label, so a field labelled "Name" gets
    /// offered a contact card whatever <c>autocomplete="off"</c> says, and nothing matches a
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
