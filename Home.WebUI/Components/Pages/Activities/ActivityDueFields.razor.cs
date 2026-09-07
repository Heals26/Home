using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace Home.WebUI.Components.Pages.Activities;

public partial class ActivityDueFields
{

    #region Properties

    [Parameter] public DateTime? Date { get; set; }
    [Parameter] public EventCallback<DateTime?> DateChanged { get; set; }
    [Parameter] public bool HasTime { get; set; }
    [Parameter] public EventCallback<bool> HasTimeChanged { get; set; }
    [Parameter] public string Time { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> TimeChanged { get; set; }

    private string DateValue => this.Date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;

    #endregion Properties

    #region Methods

    private async Task OnDateChangedAsync(string value)
    {
        var _Date = DateTime.TryParse(value, CultureInfo.InvariantCulture, out var _Parsed)
            ? _Parsed
            : (DateTime?)null;

        await this.DateChanged.InvokeAsync(_Date);

        // A time hanging off no day would be saved and never shown anywhere.
        if (_Date == null && this.HasTime)
            await this.HasTimeChangedAsync(false);
    }

    private async Task HasTimeChangedAsync(bool hasTime)
        => await this.HasTimeChanged.InvokeAsync(hasTime);

    private async Task OnTimeChangedAsync(string value)
        => await this.TimeChanged.InvokeAsync(value);

    #endregion Methods

}
