using Home.WebUI.Components.Pages.Calendar.Models;
using Home.WebUI.Components.Shared.Inputs;
using Home.WebUI.DataAccess.Calendar.Models;
using Home.WebUI.DataAccess.Users.Models;
using Home.WebUI.Infrastructure.Calendar;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Calendar;

public partial class CalendarEventEditor
{

    #region Fields

    /// <summary>
    /// Monday first, to match every other week in the app; the bitmask itself stays Sunday-based.
    /// </summary>
    private static readonly DayOfWeek[] s_WeekOrder =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
        DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
    ];

    private readonly List<HomeSelect<CalendarRecurrenceFrequency>.SelectOption> m_FrequencyOptions =
    [
        new("Does not repeat", CalendarRecurrenceFrequency.None),
        new("Every day", CalendarRecurrenceFrequency.Daily),
        new("Every week", CalendarRecurrenceFrequency.Weekly),
        new("Every month", CalendarRecurrenceFrequency.Monthly),
        new("Every year", CalendarRecurrenceFrequency.Yearly)
    ];

    #endregion Fields

    #region Properties

    [Parameter] public CalendarEventForm Form { get; set; } = new();

    /// <summary>
    /// Changes the save button's words; nothing else differs between adding and editing.
    /// </summary>
    [Parameter] public bool IsNew { get; set; }

    [Parameter] public List<UserSummaryDto> Members { get; set; } = [];
    [Parameter] public EventCallback OnSave { get; set; }

    /// <summary>
    /// What stopped the last save, shown above the form. Null when nothing did.
    /// </summary>
    [Parameter] public string? Problem { get; set; }

    [Parameter] public bool Saving { get; set; }
    [Parameter] public string Title { get; set; } = "New event";
    [Parameter] public bool Visible { get; set; }
    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }

    #endregion Properties

    #region Methods

    private Task CancelAsync()
        => this.VisibleChanged.InvokeAsync(false);

    /// <summary>
    /// Choosing weekly pre-selects the day the event starts on, so the chips show what will happen
    /// rather than seven unlit days that secretly mean the same thing.
    /// </summary>
    private void FrequencyChanged(CalendarRecurrenceFrequency frequency)
    {
        this.Form.Frequency = frequency;

        if (frequency == CalendarRecurrenceFrequency.Weekly && this.Form.DaysOfWeek == 0 && CalendarFormat.ParseDate(this.Form.StartDate) is { } _Start)
            this.Form.DaysOfWeek = 1 << (int)_Start.DayOfWeek;
    }

    private void IntervalChanged(string value)
        => this.Form.Interval = int.TryParse(value, out var _Interval) ? Math.Clamp(_Interval, 1, 99) : 1;

    private string IntervalLabel()
        => this.Form.Frequency switch
        {
            CalendarRecurrenceFrequency.Daily => "Every how many days",
            CalendarRecurrenceFrequency.Weekly => "Every how many weeks",
            CalendarRecurrenceFrequency.Monthly => "Every how many months",
            _ => "Every how many years"
        };

    /// <summary>
    /// "On the 15th" against "On the third Tuesday", worked out from the start date so the choice
    /// reads as what it will actually do.
    /// </summary>
    private List<HomeSegmentedControl<bool>.SegmentOption> MonthlyOptions()
    {
        var _Start = CalendarFormat.ParseDate(this.Form.StartDate);

        if (_Start == null)
            return [new("Same date each month", false), new("Same weekday each month", true)];

        var _Ordinal = ((_Start.Value.Day - 1) / 7) switch { 0 => "first", 1 => "second", 2 => "third", 3 => "fourth", _ => "fifth" };

        return
        [
            new($"On the {Ordinal(_Start.Value.Day)}", false),
            new($"On the {_Ordinal} {_Start.Value.DayOfWeek}", true)
        ];
    }

    private static string Ordinal(int day)
        => (day % 100) is 11 or 12 or 13
            ? $"{day}th"
            : (day % 10) switch { 1 => $"{day}st", 2 => $"{day}nd", 3 => $"{day}rd", _ => $"{day}th" };

    /// <summary>
    /// Moving the start drags an end that would otherwise fall before it, because a one-day event
    /// is the common case and asking for the end date twice is a chore.
    /// </summary>
    private void StartDateChanged(string value)
    {
        var _Start = CalendarFormat.ParseDate(value);
        var _End = CalendarFormat.ParseDate(this.Form.EndDate);

        this.Form.StartDate = value;

        if (_Start != null && (_End == null || _End < _Start))
            this.Form.EndDate = value;
    }

    private void ToggleDay(DayOfWeek day)
        => this.Form.DaysOfWeek ^= 1 << (int)day;

    private void ToggleMember(long userID)
    {
        if (!this.Form.MemberUserIDs.Remove(userID))
            _ = this.Form.MemberUserIDs.Add(userID);
    }

    #endregion Methods

}
