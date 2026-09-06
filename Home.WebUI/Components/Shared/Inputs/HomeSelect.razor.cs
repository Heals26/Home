using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Shared.Inputs;

public partial class HomeSelect<TValue>
{

    #region Records

    public record SelectOption(string Label, TValue Value);

    #endregion Records

    #region Fields

    private readonly string m_SelectID = $"home-select-{Guid.NewGuid():N}";

    #endregion Fields

    #region Properties

    /// <summary>
    /// Anything else the caller puts on the tag, which is how a filter with no visible label gets
    /// its <c>aria-label</c> without that becoming a parameter here.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? Error { get; set; }
    private string ErrorID => $"{this.m_SelectID}-error";
    /// <summary>
    /// A field in a form fills its column; a filter sitting in a row of them takes only the width
    /// its options need.
    /// </summary>
    [Parameter] public bool FullWidth { get; set; } = true;
    /// <summary>
    /// The line under the field, for anything the label cannot say in two words. It is read out
    /// with the field rather than sitting loose beside it, which is the reason it is a parameter
    /// and not a paragraph at the call site.
    /// </summary>
    [Parameter] public string? Hint { get; set; }
    private string HintID => $"{this.m_SelectID}-hint";
    [Parameter] public string? Label { get; set; }
    [Parameter] public List<SelectOption> Options { get; set; } = [];
    [Parameter] public TValue? Value { get; set; }
    [Parameter] public EventCallback<TValue> ValueChanged { get; set; }

    #endregion Properties

    #region Methods

    /// <summary>
    /// The browser hands back a string whatever the option's value was declared as. An option whose
    /// value is empty is the one that means nothing is chosen, and the conversion fails for it on a
    /// type that cannot hold nothing, which is why a failure lands on the default rather than being
    /// treated as an error.
    /// </summary>
    private async Task OnSelectionChangedAsync(ChangeEventArgs e)
    {
        var _Raw = e.Value?.ToString() ?? string.Empty;

        var _Value = BindConverter.TryConvertTo<TValue>(_Raw, CultureInfo.InvariantCulture, out var _Converted)
            ? _Converted
            : default;

        await this.ValueChanged.InvokeAsync(_Value);
    }

    /// <summary>
    /// Invariant, because these strings only ever travel between the option and the change event.
    /// A number formatted for a locale that separates with a comma would not convert back.
    /// </summary>
    private static string AsString(TValue? value)
        => value == null ? string.Empty : Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;

    private string? GetDescribedBy()
    {
        var _IDs = new List<string>();

        if (!string.IsNullOrEmpty(this.Hint))
            _IDs.Add(this.HintID);

        if (!string.IsNullOrEmpty(this.Error))
            _IDs.Add(this.ErrorID);

        return _IDs.Count == 0 ? null : string.Join(' ', _IDs);
    }

    private string GetWrapperClasses()
    {
        var _Classes = new List<string>() { "flex", "flex-col", "gap-1.5" };

        // Without this a narrow filter's label and chevron sit at the two ends of a stretched row.
        if (!this.FullWidth)
            _Classes.Add("items-start");

        if (!string.IsNullOrEmpty(this.Class))
            _Classes.Add(this.Class);

        return string.Join(' ', _Classes);
    }

    /// <summary>
    /// The same fill, border, height and focus ring as <see cref="HomeTextInput"/>, because the two
    /// sit next to each other in every form in the app.
    /// </summary>
    private string GetSelectClasses()
    {
        var _Base = "appearance-none cursor-pointer bg-ink-800 border rounded-lg pl-4 pr-10 py-3 text-sm text-ink-50 transition-colors focus:outline-none focus:ring-2 focus:ring-ink-300 focus:border-transparent disabled:opacity-50 disabled:cursor-not-allowed min-h-[48px]";
        var _Width = this.FullWidth ? "w-full" : "w-auto";
        var _Border = string.IsNullOrEmpty(this.Error) ? "border-ink-700" : "border-red-500";
        return $"{_Base} {_Width} {_Border}";
    }

    #endregion Methods

}
