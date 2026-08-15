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
    /// <summary>
    /// Prefixes the field IDs so two of these can sit on the same page without colliding.
    /// </summary>
    [Parameter] public string IdPrefix { get; set; } = "activity";
    [Parameter] public string Time { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> TimeChanged { get; set; }

    private string DateID => $"{this.IdPrefix}-due-date";
    private string DateValue => this.Date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
    private string TimeID => $"{this.IdPrefix}-due-time";

    #endregion Properties

    #region Methods

    private async Task OnDateChangedAsync(ChangeEventArgs e)
    {
        var _Value = e.Value?.ToString();
        var _Date = DateTime.TryParse(_Value, CultureInfo.InvariantCulture, out var _Parsed)
            ? _Parsed
            : (DateTime?)null;

        await this.DateChanged.InvokeAsync(_Date);

        // A time hanging off no day would be saved and never shown anywhere.
        if (_Date == null && this.HasTime)
            await this.HasTimeChangedAsync(false);
    }

    private async Task HasTimeChangedAsync(bool hasTime)
        => await this.HasTimeChanged.InvokeAsync(hasTime);

    private async Task OnTimeChangedAsync(ChangeEventArgs e)
        => await this.TimeChanged.InvokeAsync(e.Value?.ToString() ?? string.Empty);

    #endregion Methods

}
