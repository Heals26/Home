using Home.Application.UseCases.Calendar.Models;
using Home.Domain.Entities;
using Home.Domain.Enumerations;

namespace Home.Application.Infrastructure.Calendar;

/// <summary>
/// Copies what a caller said onto an event, normalising the combinations that make no sense
/// together so the table never holds a time on an all-day event or a repeat rule on a one-off.
/// Shared by create and update.
/// </summary>
public static class CalendarEventWriter
{

    #region Methods

    /// <summary>
    /// Applies the input and reconciles who is on the event. Returns the member rows that are no
    /// longer wanted so the caller can remove them; new ones are added to the collection in place,
    /// which is what lets a member who stays keep the same tracked row.
    /// </summary>
    public static IReadOnlyList<CalendarEventMember> Apply(CalendarEvent calendarEvent, ICalendarEventInput input, IReadOnlyList<User> members)
    {
        var _Repeats = input.Frequency != CalendarRecurrenceFrequency.None;

        calendarEvent.DaysOfWeek = input.Frequency == CalendarRecurrenceFrequency.Weekly ? input.DaysOfWeek & 0x7F : 0;
        calendarEvent.EndDate = input.EndDate;
        calendarEvent.EndTime = input.IsAllDay ? null : input.EndTime;
        calendarEvent.Frequency = input.Frequency;
        calendarEvent.Interval = _Repeats ? Math.Max(1, input.Interval) : 1;
        calendarEvent.IsAllDay = input.IsAllDay;
        calendarEvent.Location = Trimmed(input.Location);
        calendarEvent.Notes = Trimmed(input.Notes);
        calendarEvent.RepeatsOnWeekdayOfMonth = input.Frequency == CalendarRecurrenceFrequency.Monthly && input.RepeatsOnWeekdayOfMonth;
        calendarEvent.RepeatUntil = _Repeats ? input.RepeatUntil : null;
        calendarEvent.StartDate = input.StartDate;
        calendarEvent.StartTime = input.IsAllDay ? null : input.StartTime;
        calendarEvent.TimeZoneID = input.IsAllDay ? null : input.TimeZoneID;
        calendarEvent.Title = input.Title.Trim();

        // A weekly event with no day chosen repeats on the day it starts, never on no day.
        if (calendarEvent.Frequency == CalendarRecurrenceFrequency.Weekly && calendarEvent.DaysOfWeek == 0)
            calendarEvent.DaysOfWeek = 1 << (int)calendarEvent.StartDate.DayOfWeek;

        var _WantedIDs = members.Select(u => u.UserID).ToHashSet();
        var _Removed = calendarEvent.Members.Where(m => !_WantedIDs.Contains(m.UserID)).ToList();

        foreach (var _Member in _Removed)
            _ = calendarEvent.Members.Remove(_Member);

        var _PresentIDs = calendarEvent.Members.Select(m => m.UserID).ToHashSet();

        foreach (var _User in members.Where(u => !_PresentIDs.Contains(u.UserID)))
            calendarEvent.Members.Add(new CalendarEventMember() { CalendarEvent = calendarEvent, User = _User, UserID = _User.UserID });

        return _Removed;
    }

    private static string? Trimmed(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    #endregion Methods

}
