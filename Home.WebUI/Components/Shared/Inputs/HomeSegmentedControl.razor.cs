using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Shared.Inputs;

public partial class HomeSegmentedControl<TValue>
{

    #region Records

    public record SegmentOption(string Label, TValue Value);

    #endregion Records

    #region Properties

    [Parameter] public string? Class { get; set; }
    [Parameter] public List<SegmentOption> Options { get; set; } = [];
    [Parameter] public TValue? Value { get; set; }
    [Parameter] public EventCallback<TValue> ValueChanged { get; set; }

    #endregion Properties

    #region Methods

    private async Task SelectAsync(TValue value)
        => await this.ValueChanged.InvokeAsync(value);

    private string GetOptionClasses(TValue optionValue)
    {
        var _IsSelected = EqualityComparer<TValue>.Default.Equals(this.Value, optionValue);
        var _Base = "px-4 py-2 text-sm font-medium rounded-md transition-all duration-150 min-h-[40px]";
        var _State = _IsSelected ? "bg-ink-50 text-ink-950 shadow-sm" : "text-ink-400 hover:text-ink-200";
        return $"{_Base} {_State}";
    }

    #endregion Methods

}
