using Home.WebUI.DataAccess.Calendar.Models;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Calendar;

public partial class CalendarItemChip
{

    #region Properties

    /// <summary>
    /// One line, for a month cell. The full form adds the time and who is on it.
    /// </summary>
    [Parameter] public bool Compact { get; set; }

    [Parameter] public CalendarItemDto Item { get; set; } = new();
    [Parameter] public EventCallback<CalendarItemDto> OnClick { get; set; }

    #endregion Properties

    #region Methods

    private string Classes()
        => this.Compact
            ? "flex w-full min-w-0 items-center gap-1.5 rounded-md px-1.5 py-0.5 min-h-[24px] text-left enabled:hover:bg-ink-800"
            : "flex w-full min-w-0 items-stretch gap-2.5 rounded-xl border border-ink-800 bg-ink-900 px-3 py-2 min-h-[48px] text-left enabled:hover:border-ink-600";

    private string DotClass()
        => this.Item.Kind switch
        {
            CalendarItemKind.Meal => "bg-recipes",
            CalendarItemKind.Task => this.Item.IsDone ? "bg-ink-600" : "bg-week",
            CalendarItemKind.Subscribed => "bg-ink-500",
            _ => "bg-calendar"
        };

    #endregion Methods

}
