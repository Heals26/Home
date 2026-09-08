using Home.Application.Services.Calendar;
using Home.Application.Services.EntityLogic.Calendar;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;
using Home.Domain.Enumerations;

namespace Home.Application.Infrastructure.Calendar;

/// <summary>
/// The refresh shared by adding a subscription, the Refresh button and the background runner. The
/// feed is the source of truth, so a refresh replaces the subscription's rows wholesale rather
/// than reconciling them: nothing can drift from the feed for longer than one refresh.
/// </summary>
public class CalendarSubscriptionLogic(
    ICalendarFeedService calendarFeedService,
    IPersistenceContext persistenceContext,
    TimeProvider timeProvider)
    : ICalendarSubscriptionLogic
{

    #region Methods

    async Task<bool> ICalendarSubscriptionLogic.RefreshAsync(CalendarSubscription subscription, CancellationToken cancellationToken)
    {
        var _Now = timeProvider.GetUtcNow().UtcDateTime;

        var _Feed = await calendarFeedService.ReadAsync(
            subscription.Url,
            _Now.AddDays(-CalendarValues.SubscriptionWindowDaysBack),
            _Now.AddDays(CalendarValues.SubscriptionWindowDaysForward),
            cancellationToken);

        if (_Feed == null)
        {
            subscription.LastError = "The calendar could not be read. Home is showing what it last fetched and will try again shortly.";
            return false;
        }

        var _Existing = persistenceContext.GetEntities<CalendarEvent>()
            .Where(e => e.Subscription != null && e.Subscription.CalendarSubscriptionID == subscription.CalendarSubscriptionID)
            .ToList();

        persistenceContext.RemoveRange(_Existing);

        foreach (var _Occurrence in _Feed.Occurrences)
            persistenceContext.Add(ToEvent(subscription, _Occurrence));

        // A blank name means "call it what the feed calls itself".
        if (string.IsNullOrWhiteSpace(subscription.Name))
            subscription.Name = Truncate(string.IsNullOrWhiteSpace(_Feed.Name) ? "Calendar" : _Feed.Name, 100);

        subscription.LastError = null;
        subscription.LastFetchedUTC = _Now;

        return true;
    }

    private static CalendarEvent ToEvent(CalendarSubscription subscription, CalendarFeedOccurrence occurrence)
        => new()
        {
            EndDate = occurrence.EndDate,
            EndTime = occurrence.IsAllDay ? null : occurrence.EndTime,
            ExternalUID = Truncate(occurrence.ExternalUID, 500),
            Frequency = CalendarRecurrenceFrequency.None,
            Household = subscription.Household,
            Interval = 1,
            IsAllDay = occurrence.IsAllDay,
            Location = occurrence.Location == null ? null : Truncate(occurrence.Location, 250),
            StartDate = occurrence.StartDate,
            StartTime = occurrence.IsAllDay ? null : occurrence.StartTime,
            Subscription = subscription,
            TimeZoneID = occurrence.IsAllDay ? null : CalendarValues.UtcTimeZoneID,
            Title = Truncate(string.IsNullOrWhiteSpace(occurrence.Title) ? "(Untitled)" : occurrence.Title, 250)
        };

    private static string Truncate(string value, int length)
        => value.Length <= length ? value : value[..length];

    #endregion Methods

}
