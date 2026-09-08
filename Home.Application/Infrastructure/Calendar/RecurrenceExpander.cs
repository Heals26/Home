using Home.Domain.Entities;
using Home.Domain.Enumerations;

namespace Home.Application.Infrastructure.Calendar;

/// <summary>
/// Turns an event's five recurrence columns into the dates its occurrences start on. Pure and
/// static so it can be pinned by tests: recurrence is exactly the kind of logic that looks right
/// and is wrong on the fifth Monday of a month.
/// <para>
/// Dates are in the event's own zone. Skipped occurrences (<see cref="CalendarEvent.Exceptions"/>)
/// are removed here, so the caller must have loaded them.
/// </para>
/// </summary>
public static class RecurrenceExpander
{

    #region Methods

    /// <summary>
    /// Start dates, earliest first, of every occurrence that could touch the window. An
    /// occurrence spanning several days touches the window if any of its days does.
    /// </summary>
    public static IEnumerable<DateOnly> StartDates(CalendarEvent calendarEvent, DateOnly windowStart, DateOnly windowEnd)
    {
        var _Length = Math.Max(0, calendarEvent.EndDate.DayNumber - calendarEvent.StartDate.DayNumber);
        var _Earliest = windowStart.AddDays(-_Length);
        var _Last = calendarEvent.RepeatUntil is { } _Until && _Until < windowEnd ? _Until : windowEnd;
        var _Skipped = calendarEvent.Exceptions.Select(e => e.OccurrenceDate).ToHashSet();

        return Candidates(calendarEvent, _Last)
            .TakeWhile(d => d <= _Last)
            .Where(d => d >= _Earliest && !_Skipped.Contains(d))
            .Take(CalendarValues.MaximumOccurrencesPerSeries);
    }

    private static IEnumerable<DateOnly> Candidates(CalendarEvent calendarEvent, DateOnly last)
    {
        var _Start = calendarEvent.StartDate;
        var _Interval = Math.Max(1, calendarEvent.Interval);

        switch (calendarEvent.Frequency)
        {
            case CalendarRecurrenceFrequency.Daily:
                for (var _Date = _Start; _Date <= last; _Date = _Date.AddDays(_Interval))
                    yield return _Date;
                break;

            case CalendarRecurrenceFrequency.Weekly:
                foreach (var _Date in WeeklyCandidates(_Start, calendarEvent.DaysOfWeek, _Interval, last))
                    yield return _Date;
                break;

            case CalendarRecurrenceFrequency.Monthly:
                foreach (var _Date in MonthlyCandidates(_Start, calendarEvent.RepeatsOnWeekdayOfMonth, _Interval, last))
                    yield return _Date;
                break;

            case CalendarRecurrenceFrequency.Yearly:
                foreach (var _Date in YearlyCandidates(_Start, _Interval, last))
                    yield return _Date;
                break;

            default:
                yield return _Start;
                break;
        }
    }

    private static DateOnly? DayOfMonth(DateOnly firstOfMonth, int day)
        => day <= DateTime.DaysInMonth(firstOfMonth.Year, firstOfMonth.Month)
            ? new DateOnly(firstOfMonth.Year, firstOfMonth.Month, day)
            : null;

    private static IEnumerable<DateOnly> MonthlyCandidates(DateOnly start, bool byWeekday, int interval, DateOnly last)
    {
        // "First Monday": which weekday, and which one of them in the month.
        var _Ordinal = ((start.Day - 1) / 7) + 1;

        for (var _Month = new DateOnly(start.Year, start.Month, 1); _Month <= last; _Month = _Month.AddMonths(interval))
        {
            var _Date = byWeekday
                ? NthWeekdayOfMonth(_Month, start.DayOfWeek, _Ordinal)
                : DayOfMonth(_Month, start.Day);

            // A month without a 31st, or without a fifth Tuesday, simply has no occurrence.
            if (_Date == null || _Date < start)
                continue;

            yield return _Date.Value;
        }
    }

    private static DateOnly? NthWeekdayOfMonth(DateOnly firstOfMonth, DayOfWeek dayOfWeek, int ordinal)
    {
        var _Offset = ((int)dayOfWeek - (int)firstOfMonth.DayOfWeek + 7) % 7;
        var _Day = 1 + _Offset + ((ordinal - 1) * 7);

        return _Day <= DateTime.DaysInMonth(firstOfMonth.Year, firstOfMonth.Month)
            ? new DateOnly(firstOfMonth.Year, firstOfMonth.Month, _Day)
            : null;
    }

    private static IEnumerable<DateOnly> WeeklyCandidates(DateOnly start, int daysOfWeek, int interval, DateOnly last)
    {
        // Zero means "the day it starts on", so a weekly event never silently fires on no day.
        var _Mask = daysOfWeek == 0 ? 1 << (int)start.DayOfWeek : daysOfWeek;

        // Weeks are counted from the Sunday of the start date's week, bit 0 being Sunday.
        var _Week = start.AddDays(-(int)start.DayOfWeek);

        for (; _Week <= last; _Week = _Week.AddDays(7 * interval))
        {
            for (var _Day = 0; _Day < 7; _Day++)
            {
                if ((_Mask & (1 << _Day)) == 0)
                    continue;

                var _Date = _Week.AddDays(_Day);

                if (_Date < start)
                    continue;

                if (_Date > last)
                    yield break;

                yield return _Date;
            }
        }
    }

    private static IEnumerable<DateOnly> YearlyCandidates(DateOnly start, int interval, DateOnly last)
    {
        for (var _Year = start.Year; _Year <= last.Year; _Year += interval)
        {
            // 29 February only happens in leap years; the other years have no occurrence.
            if (start.Month == 2 && start.Day == 29 && !DateTime.IsLeapYear(_Year))
                continue;

            yield return new DateOnly(_Year, start.Month, start.Day);
        }
    }

    #endregion Methods

}
