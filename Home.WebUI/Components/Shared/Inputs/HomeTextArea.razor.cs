using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Shared.Inputs;

public partial class HomeTextArea
{

    #region Fields

    private readonly string m_TextAreaID = $"home-textarea-{Guid.NewGuid():N}";

    /// <summary>
    /// What the box is rendered with, which is not always what <see cref="Value"/> says.
    /// </summary>
    private string m_Shown = string.Empty;

    /// <summary>
    /// What the browser last said the box holds, or null when the caller last set it.
    /// </summary>
    private string? m_Typed;

    /// <summary>
    /// Counts the times the caller has written to the box, and keys the element so a write lands
    /// even when it carries the same text as the one before it.
    /// </summary>
    private int m_Written;

    #endregion Fields

    #region Properties

    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? Error { get; set; }
    private string ErrorID => $"{this.m_TextAreaID}-error";
    [Parameter] public string? Hint { get; set; }
    private string HintID => $"{this.m_TextAreaID}-hint";
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public int Rows { get; set; } = 3;
    [Parameter] public string Value { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> ValueChanged { get; set; }

    #endregion Properties

    #region Lifecycle Methods

    /// <summary>
    /// The browser owns the box while someone is typing in it, for the reason written up on
    /// <see cref="HomeTextInput"/>: on a slow connection the render answering one keystroke lands
    /// after the next has been typed, and putting the value back eats it.
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

    private async Task OnInputChangedAsync(ChangeEventArgs e)
    {
        this.m_Typed = e.Value?.ToString() ?? string.Empty;

        await this.ValueChanged.InvokeAsync(this.m_Typed);
    }

    private string? GetDescribedBy()
    {
        var _IDs = new List<string>();

        if (!string.IsNullOrEmpty(this.Hint))
            _IDs.Add(this.HintID);

        if (!string.IsNullOrEmpty(this.Error))
            _IDs.Add(this.ErrorID);

        return _IDs.Count == 0 ? null : string.Join(' ', _IDs);
    }

    /// <summary>
    /// Must stay in step with <see cref="HomeTextInput"/>, minus the minimum height, which
    /// <see cref="Rows"/> decides here.
    /// </summary>
    private string GetTextAreaClasses()
    {
        var _Base = "w-full resize-y bg-ink-800 border rounded-lg px-4 py-3 text-sm text-ink-50 placeholder-ink-500 transition-colors focus:outline-none focus:ring-2 focus:ring-ink-300 focus:border-transparent disabled:opacity-50 disabled:cursor-not-allowed";
        var _Border = string.IsNullOrEmpty(this.Error) ? "border-ink-700" : "border-red-500";
        return $"{_Base} {_Border}";
    }

    #endregion Methods

}
