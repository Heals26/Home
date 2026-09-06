using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Shared.Inputs;

public partial class HomeTextArea
{

    #region Fields

    private readonly string m_TextAreaID = $"home-textarea-{Guid.NewGuid():N}";

    #endregion Fields

    #region Properties

    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? Error { get; set; }
    private string ErrorID => $"{this.m_TextAreaID}-error";
    /// <summary>
    /// The line under the field, for anything the label cannot say in two words. It is read out
    /// with the field rather than sitting loose beside it, which is the reason it is a parameter
    /// and not a paragraph at the call site.
    /// </summary>
    [Parameter] public string? Hint { get; set; }
    private string HintID => $"{this.m_TextAreaID}-hint";
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public int Rows { get; set; } = 3;
    [Parameter] public string Value { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> ValueChanged { get; set; }

    #endregion Properties

    #region Methods

    private async Task OnInputChangedAsync(ChangeEventArgs e)
        => await this.ValueChanged.InvokeAsync(e.Value?.ToString() ?? string.Empty);

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
    /// The same fill, border and focus ring as <see cref="HomeTextInput"/>. Height comes from
    /// <see cref="Rows"/> rather than a minimum, and only the vertical handle is offered, because
    /// dragging a field wider than its column is never what was wanted.
    /// </summary>
    private string GetTextAreaClasses()
    {
        var _Base = "w-full resize-y bg-ink-800 border rounded-lg px-4 py-3 text-sm text-ink-50 placeholder-ink-500 transition-colors focus:outline-none focus:ring-2 focus:ring-ink-300 focus:border-transparent disabled:opacity-50 disabled:cursor-not-allowed";
        var _Border = string.IsNullOrEmpty(this.Error) ? "border-ink-700" : "border-red-500";
        return $"{_Base} {_Border}";
    }

    #endregion Methods

}
