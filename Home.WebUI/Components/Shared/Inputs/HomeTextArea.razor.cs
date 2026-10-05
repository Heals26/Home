using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Home.WebUI.Components.Shared.Inputs;

public partial class HomeTextArea
{

    #region Fields

    private readonly string m_TextAreaID = $"home-textarea-{Guid.NewGuid():N}";
    private ElementReference m_TextArea;

    /// <summary>
    /// A write from the caller that the render cannot deliver, waiting to be put into the box once
    /// the render has landed. Null when there is nothing waiting.
    /// </summary>
    private string? m_PendingWrite;

    /// <summary>
    /// What the box is rendered with, which is not always what <see cref="Value"/> says.
    /// </summary>
    private string m_Shown = string.Empty;

    /// <summary>
    /// What the browser last said the box holds, or null when the caller last set it.
    /// </summary>
    private string? m_Typed;

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
    /// The browser owns the box while someone is typing in it, and a write from the caller goes in
    /// once the render lands, both for the reasons written up on <see cref="HomeTextInput"/>.
    /// </summary>
    protected override void OnParametersSet()
    {
        if (this.m_Typed != null && this.Value == this.m_Typed)
            return;

        if (this.m_Typed != null)
            this.m_PendingWrite = this.Value;

        this.m_Shown = this.Value;
        this.m_Typed = null;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (this.m_PendingWrite is not { } _Write)
            return;

        this.m_PendingWrite = null;

        // A dead circuit cannot reach the browser, and that is never worth an error on screen.
        try { await this.JS.InvokeVoidAsync("homeInput.write", this.m_TextArea, _Write); } catch { }
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
