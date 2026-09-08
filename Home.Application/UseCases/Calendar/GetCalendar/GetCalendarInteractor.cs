using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.Calendar;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Application.UseCases.Calendar.Models;
using Home.Domain.Entities;
using Home.Domain.Enumerations;

namespace Home.Application.UseCases.Calendar.GetCalendar;

/// <summary>
/// The one read behind the calendar and the dashboard: a projection across the household's own
/// events, its planned meals and its dated tasks, each placed on the viewer's local day. Nothing
/// is stored twice; a meal stays a meal and a chore stays a chore, and the item says which.
/// </summary>
internal class GetCalendarInteractor : IInteractor<GetCalendarInputPort, IGetCalendarOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetCalendarInputPort inputPort,
        IGetCalendarOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();
        var _Zone = TimeZoneResolver.Resolve(inputPort.TimeZoneID);

        var _Items = new List<CalendarItem>();

        _Items.AddRange(EventItems(_PersistenceContext, _Household.HouseholdID, inputPort.FromDate, inputPort.ToDate, _Zone));
        _Items.AddRange(MealItems(_PersistenceContext, _Household.HouseholdID, inputPort.FromDate, inputPort.ToDate));
        _Items.AddRange(TaskItems(_PersistenceContext, _Household.HouseholdID, inputPort.FromDate, inputPort.ToDate));

        var _Days = new List<CalendarDay>();

        for (var _Date = inputPort.FromDate; _Date <= inputPort.ToDate; _Date = _Date.AddDays(1))
        {
            _Days.Add(new CalendarDay(_Date, [.. _Items
                .Where(i => i.Date == _Date)
                // All-day things first, in a fixed order of kind; then the timed things by time.
                .OrderBy(i => i.StartsAt.HasValue)
                .ThenBy(i => i.StartsAt)
                .ThenBy(i => i.Kind)
                .ThenBy(i => i.Title)]));
        }

        await outputPort.PresentCalendarAsync(_Days, cancellationToken);
    }

    private static IEnumerable<CalendarItem> EventItems(
        IPersistenceContext persistenceContext,
        long householdID,
        DateOnly fromDate,
        DateOnly toDate,
        TimeZoneInfo zone)
    {
        // A day either side, because a timed event lands on a different viewer-local day than its
        // own zone's when the two zones straddle midnight.
        var _WindowStart = fromDate.AddDays(-1);
        var _WindowEnd = toDate.AddDays(1);

        // A series that ended before the window is out, allowing for a multi-day final occurrence.
        var _SeriesFloor = _WindowStart.AddDays(-CalendarValues.MaximumWindowDays);

        var _Events = persistenceContext.GetEntities<CalendarEvent>()
            .Where(e => e.Household.HouseholdID == householdID
                && e.StartDate <= _WindowEnd
                && (e.Frequency == CalendarRecurrenceFrequency.None
                    ? e.EndDate >= _WindowStart
                    : e.RepeatUntil == null || e.RepeatUntil >= _SeriesFloor))
            .Select(e => new
            {
                Event = e,
                e.Exceptions,
                Members = e.Members.Select(m => new { Member = m, m.User }),
                e.Subscription
            })
            .ToList()
            .Select(e => e.Event)
            .ToList();

        foreach (var _Event in _Events)
        {
            var _Members = _Event.Members.OrderBy(m => m.User.UserName).ToList();
            var _People = _Members.Select(m => m.User.UserName).ToList();
            var _PersonIDs = _Members.Select(m => m.UserID).ToList();

            foreach (var _Occurrence in CalendarOccurrences.Expand(_Event, _WindowStart, _WindowEnd))
            {
                foreach (var _Item in OccurrenceItems(_Occurrence, _People, _PersonIDs, fromDate, toDate, zone))
                    yield return _Item;
            }
        }
    }

    /// <summary>
    /// One item per viewer-local day the occurrence covers, clipped to the window.
    /// </summary>
    private static IEnumerable<CalendarItem> OccurrenceItems(
        CalendarOccurrence occurrence,
        IReadOnlyList<string> people,
        IReadOnlyList<long> personIDs,
        DateOnly fromDate,
        DateOnly toDate,
        TimeZoneInfo zone)
    {
        var _Event = occurrence.Event;
        var _IsTimed = occurrence.StartUTC != null && occurrence.EndUTC != null;

        var _LocalStart = _IsTimed ? TimeZoneResolver.ToLocal(occurrence.StartUTC!.Value, zone) : occurrence.StartDate.ToDateTime(TimeOnly.MinValue);
        var _LocalEnd = _IsTimed ? TimeZoneResolver.ToLocal(occurrence.EndUTC!.Value, zone) : occurrence.EndDate.ToDateTime(TimeOnly.MinValue);

        var _FirstDay = DateOnly.FromDateTime(_LocalStart);

        // An event ending exactly at midnight ends on the day before, not on a day it never touches.
        var _LastDay = _IsTimed && _LocalEnd.TimeOfDay == TimeSpan.Zero && _LocalEnd > _LocalStart
            ? DateOnly.FromDateTime(_LocalEnd).AddDays(-1)
            : DateOnly.FromDateTime(_LocalEnd);

        if (_LastDay < _FirstDay)
            _LastDay = _FirstDay;

        var _From = _FirstDay > fromDate ? _FirstDay : fromDate;
        var _To = _LastDay < toDate ? _LastDay : toDate;

        for (var _Date = _From; _Date <= _To; _Date = _Date.AddDays(1))
        {
            yield return new CalendarItem()
            {
                Date = _Date,
                EndDate = _LastDay,
                EndsAt = _IsTimed && _Date == _LastDay ? TimeOnly.FromDateTime(_LocalEnd) : null,
                EndUTC = occurrence.EndUTC,
                ID = _Event.CalendarEventID,
                IsAllDay = !_IsTimed,
                IsReadOnly = _Event.Subscription != null,
                IsRecurring = _Event.Frequency != CalendarRecurrenceFrequency.None,
                Kind = _Event.Subscription == null ? CalendarItemKind.Event : CalendarItemKind.Subscribed,
                Location = _Event.Location,
                OccurrenceDate = occurrence.StartDate,
                People = people,
                PersonUserIDs = personIDs,
                StartDate = _FirstDay,
                // A day the event runs into starts at midnight for sorting: before anything timed.
                StartsAt = !_IsTimed ? null : _Date == _FirstDay ? TimeOnly.FromDateTime(_LocalStart) : TimeOnly.MinValue,
                StartUTC = occurrence.StartUTC,
                Subtitle = _Event.Subscription?.Name,
                Title = _Event.Title
            };
        }
    }

    private static IEnumerable<CalendarItem> MealItems(
        IPersistenceContext persistenceContext,
        long householdID,
        DateOnly fromDate,
        DateOnly toDate)
    {
        var _From = fromDate.ToDateTime(TimeOnly.MinValue);
        var _To = toDate.ToDateTime(TimeOnly.MinValue);

        var _Entries = persistenceContext.GetEntities<MealPlanEntry>()
            .Where(e => e.Household.HouseholdID == householdID && e.Date >= _From && e.Date <= _To)
            .Select(e => new
            {
                Entry = e,
                e.MealSlot,
                e.Recipe
            })
            .ToList()
            .Select(e => e.Entry);

        foreach (var _Entry in _Entries)
        {
            var _Date = DateOnly.FromDateTime(_Entry.Date);

            yield return new CalendarItem()
            {
                Date = _Date,
                EndDate = _Date,
                ID = _Entry.MealPlanEntryID,
                IsAllDay = true,
                Kind = CalendarItemKind.Meal,
                RecipeID = _Entry.Recipe?.RecipeID,
                StartDate = _Date,
                // A meal with a usual time sits in the day's timeline at that time; one without
                // sits with the all-day things.
                StartsAt = _Entry.MealSlot?.StartsAt is { } _StartsAt ? TimeOnly.FromTimeSpan(_StartsAt) : null,
                Subtitle = _Entry.MealSlot?.Name,
                Title = _Entry.Recipe?.Name ?? _Entry.Title ?? string.Empty
            };
        }
    }

    private static IEnumerable<CalendarItem> TaskItems(
        IPersistenceContext persistenceContext,
        long householdID,
        DateOnly fromDate,
        DateOnly toDate)
    {
        // DueDateUTC holds the day the user picked at midnight, so its date part is the calendar
        // day (see the 8 Sep 2026 decision on time). The window is a half-open day range.
        var _From = fromDate.ToDateTime(TimeOnly.MinValue);
        var _To = toDate.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var _Activities = persistenceContext.GetEntities<Activity>()
            .Where(a => a.Household.HouseholdID == householdID
                && a.DueDateUTC != null
                && a.DueDateUTC >= _From
                && a.DueDateUTC < _To)
            .Select(a => new
            {
                Activity = a,
                a.CompletedByUser,
                a.User
            })
            .ToList()
            .Select(a => a.Activity);

        foreach (var _Activity in _Activities)
        {
            var _Date = DateOnly.FromDateTime(_Activity.DueDateUTC!.Value);

            yield return new CalendarItem()
            {
                Date = _Date,
                EndDate = _Date,
                ID = _Activity.ActivityID,
                IsAllDay = _Activity.DueTime == null,
                IsDone = _Activity.CompletedDateUTC != null,
                Kind = CalendarItemKind.Task,
                StartDate = _Date,
                PersonUserIDs = _Activity.User == null ? [] : [_Activity.User.UserID],
                StartsAt = _Activity.DueTime is { } _DueTime ? TimeOnly.FromTimeSpan(_DueTime) : null,
                Subtitle = _Activity.CompletedByUser != null ? $"Done by {_Activity.CompletedByUser.UserName}" : _Activity.User?.UserName,
                Title = _Activity.Title
            };
        }
    }

    #endregion Methods

}
