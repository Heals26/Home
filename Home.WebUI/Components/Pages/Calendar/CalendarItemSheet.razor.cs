using Home.WebUI.DataAccess.Calendar.Models;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Calendar;

public partial class CalendarItemSheet
{

    #region Properties

    /// <summary>
    /// An action is in flight; the buttons wait for it.
    /// </summary>
    [Parameter] public bool Busy { get; set; }

    /// <summary>
    /// The thing whose sheet is open, or null for closed.
    /// </summary>
    [Parameter] public CalendarItemDto? Item { get; set; }

    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnCompleteTask { get; set; }

    /// <summary>
    /// True removes the whole event; false skips only this occurrence of a series.
    /// </summary>
    [Parameter] public EventCallback<bool> OnDelete { get; set; }

    /// <summary>
    /// True edits the whole event; false changes only this occurrence of a series.
    /// </summary>
    [Parameter] public EventCallback<bool> OnEdit { get; set; }

    #endregion Properties

    #region Methods

    private string SubtitleLabel(CalendarItemDto item)
        => item.Kind switch
        {
            CalendarItemKind.Meal => item.Subtitle!,
            CalendarItemKind.Task => $"For {item.Subtitle}",
            CalendarItemKind.Subscribed => $"From {item.Subtitle}",
            _ => item.Subtitle!
        };

    #endregion Methods

}
