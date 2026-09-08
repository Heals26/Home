using FluentAssertions;
using Home.Application.Infrastructure.Calendar;
using Home.Domain.Entities;
using Home.Domain.Enumerations;

namespace Home.Application.Tests.Infrastructure.Calendar;

/// <summary>
/// Pins the recurrence rules the 8 Sep 2026 decision wrote down, including what they refuse to
/// do: a month without a 31st has no occurrence, and 29 February only happens in leap years.
/// </summary>
public class RecurrenceExpanderTests
{

    #region Methods

    private static CalendarEvent Build(
        DateOnly startDate,
        CalendarRecurrenceFrequency frequency,
        int interval = 1,
        int daysOfWeek = 0,
        bool repeatsOnWeekdayOfMonth = false,
        DateOnly? repeatUntil = null,
        int lengthInDays = 0,
        params DateOnly[] skipped)
        => new()
        {
            DaysOfWeek = daysOfWeek,
            EndDate = startDate.AddDays(lengthInDays),
            Exceptions = [.. skipped.Select(d => new CalendarEventException() { OccurrenceDate = d })],
            Frequency = frequency,
            Interval = interval,
            RepeatsOnWeekdayOfMonth = repeatsOnWeekdayOfMonth,
            RepeatUntil = repeatUntil,
            StartDate = startDate,
            Title = "Test"
        };

    private static DateOnly D(int year, int month, int day)
        => new(year, month, day);

    [Fact]
    public void StartDates_AOneOffHappensOnceAndOnlyInsideTheWindow()
    {
        var _Event = Build(D(2026, 9, 15), CalendarRecurrenceFrequency.None);

        _ = RecurrenceExpander.StartDates(_Event, D(2026, 9, 1), D(2026, 9, 30)).Should().Equal(D(2026, 9, 15));
        _ = RecurrenceExpander.StartDates(_Event, D(2026, 10, 1), D(2026, 10, 31)).Should().BeEmpty();
    }

    [Fact]
    public void StartDates_AMultiDayEventStartingBeforeTheWindowStillTouchesIt()
    {
        var _Event = Build(D(2026, 8, 30), CalendarRecurrenceFrequency.None, lengthInDays: 3);

        _ = RecurrenceExpander.StartDates(_Event, D(2026, 9, 1), D(2026, 9, 30)).Should().Equal(
            [D(2026, 8, 30)],
            "it runs to 2 September, so the window sees it");
    }

    [Fact]
    public void StartDates_WeeklyOnChosenDaysStartsFromTheStartDateNotTheWeek()
    {
        // Tuesday 1 September 2026; Monday (bit 1) and Thursday (bit 4) chosen.
        var _Event = Build(D(2026, 9, 1), CalendarRecurrenceFrequency.Weekly, daysOfWeek: (1 << 1) | (1 << 4));

        _ = RecurrenceExpander.StartDates(_Event, D(2026, 9, 1), D(2026, 9, 14)).Should().Equal(
            [D(2026, 9, 3), D(2026, 9, 7), D(2026, 9, 10), D(2026, 9, 14)],
            "the Monday before the start date is not an occurrence");
    }

    [Fact]
    public void StartDates_WeeklyWithNoDayChosenRepeatsOnTheStartDay()
    {
        var _Event = Build(D(2026, 9, 1), CalendarRecurrenceFrequency.Weekly);

        _ = RecurrenceExpander.StartDates(_Event, D(2026, 9, 1), D(2026, 9, 30)).Should().Equal(
            D(2026, 9, 1), D(2026, 9, 8), D(2026, 9, 15), D(2026, 9, 22), D(2026, 9, 29));
    }

    [Fact]
    public void StartDates_FortnightlySkipsAlternateWeeks()
    {
        var _Event = Build(D(2026, 9, 1), CalendarRecurrenceFrequency.Weekly, interval: 2);

        _ = RecurrenceExpander.StartDates(_Event, D(2026, 9, 1), D(2026, 9, 30)).Should().Equal(
            D(2026, 9, 1), D(2026, 9, 15), D(2026, 9, 29));
    }

    [Fact]
    public void StartDates_MonthlyOnTheThirtyFirstSkipsShortMonths()
    {
        var _Event = Build(D(2026, 1, 31), CalendarRecurrenceFrequency.Monthly);

        _ = RecurrenceExpander.StartDates(_Event, D(2026, 1, 1), D(2026, 6, 30)).Should().Equal(
            [D(2026, 1, 31), D(2026, 3, 31), D(2026, 5, 31)],
            "February, April and June have no 31st, so they have no occurrence");
    }

    [Fact]
    public void StartDates_MonthlyOnTheFirstMondayFollowsTheWeekday()
    {
        // 7 September 2026 is the first Monday of September.
        var _Event = Build(D(2026, 9, 7), CalendarRecurrenceFrequency.Monthly, repeatsOnWeekdayOfMonth: true);

        _ = RecurrenceExpander.StartDates(_Event, D(2026, 9, 1), D(2026, 12, 31)).Should().Equal(
            D(2026, 9, 7), D(2026, 10, 5), D(2026, 11, 2), D(2026, 12, 7));
    }

    [Fact]
    public void StartDates_MonthlyOnTheFifthWeekdaySkipsMonthsWithoutOne()
    {
        // 29 September 2026 is the fifth Tuesday of September. October 2026 has only four.
        var _Event = Build(D(2026, 9, 29), CalendarRecurrenceFrequency.Monthly, repeatsOnWeekdayOfMonth: true);

        _ = RecurrenceExpander.StartDates(_Event, D(2026, 9, 1), D(2026, 12, 31)).Should().Equal(
            [D(2026, 9, 29), D(2026, 12, 29)],
            "October and November 2026 have no fifth Tuesday");
    }

    [Fact]
    public void StartDates_YearlyOnALeapDayOnlyHappensInLeapYears()
    {
        var _Event = Build(D(2024, 2, 29), CalendarRecurrenceFrequency.Yearly);

        _ = RecurrenceExpander.StartDates(_Event, D(2024, 1, 1), D(2028, 12, 31)).Should().Equal(
            D(2024, 2, 29), D(2028, 2, 29));
    }

    [Fact]
    public void StartDates_StopsAtRepeatUntil()
    {
        var _Event = Build(D(2026, 9, 1), CalendarRecurrenceFrequency.Daily, repeatUntil: D(2026, 9, 3));

        _ = RecurrenceExpander.StartDates(_Event, D(2026, 9, 1), D(2026, 9, 30)).Should().Equal(
            D(2026, 9, 1), D(2026, 9, 2), D(2026, 9, 3));
    }

    [Fact]
    public void StartDates_LeavesOutSkippedOccurrences()
    {
        var _Event = Build(D(2026, 9, 1), CalendarRecurrenceFrequency.Daily, skipped: [D(2026, 9, 2), D(2026, 9, 4)]);

        _ = RecurrenceExpander.StartDates(_Event, D(2026, 9, 1), D(2026, 9, 5)).Should().Equal(
            D(2026, 9, 1), D(2026, 9, 3), D(2026, 9, 5));
    }

    [Fact]
    public void Expand_ATimedRepeatKeepsItsWallClockAcrossDaylightSaving()
    {
        // Sydney goes onto daylight saving on 4 October 2026. 4pm stays 4pm; the UTC instant moves.
        var _Event = Build(D(2026, 9, 29), CalendarRecurrenceFrequency.Weekly);
        _Event.StartTime = new TimeOnly(16, 0);
        _Event.EndTime = new TimeOnly(17, 0);
        _Event.TimeZoneID = "Australia/Sydney";

        var _Occurrences = CalendarOccurrences.Expand(_Event, D(2026, 9, 29), D(2026, 10, 6)).ToList();

        _ = _Occurrences.Select(o => o.StartUTC).Should().Equal(
            [new DateTime(2026, 9, 29, 6, 0, 0), new DateTime(2026, 10, 6, 5, 0, 0)],
            "4pm AEST is 06:00 UTC and 4pm AEDT is 05:00 UTC");
    }

    #endregion Methods

}
