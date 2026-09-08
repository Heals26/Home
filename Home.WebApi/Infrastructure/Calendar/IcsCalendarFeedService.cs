// Home.WebApi has nullable disabled project-wide, but ICalendarFeedService's contract is
// nullable-aware (a null feed means "could not be read"). Opting this file in keeps that meaning.
#nullable enable

using Home.Application.Services.Calendar;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using IcsCalendar = Ical.Net.Calendar;

namespace Home.WebApi.Infrastructure.Calendar;

/// <summary>
/// Reads a published iCalendar feed and expands it into occurrences with Ical.Net. The library's
/// types never leave this file: the use cases see <see cref="CalendarFeedOccurrence"/> and nothing
/// else. Every call round-trips to the internet, so a feed being unreachable or malformed is an
/// expected outcome and comes back as null rather than an exception.
/// </summary>
internal class IcsCalendarFeedService(
    HttpClient httpClient,
    ILogger<IcsCalendarFeedService> logger) : ICalendarFeedService
{

    #region Methods

    public async Task<CalendarFeed?> ReadAsync(
        string url,
        DateTime windowStartUTC,
        DateTime windowEndUTC,
        CancellationToken cancellationToken)
    {
        var _Text = await this.FetchAsync(url, cancellationToken);

        if (_Text == null)
            return null;

        try
        {
            var _Calendar = IcsCalendar.Load(_Text);

            if (_Calendar == null)
            {
                logger.LogWarning("The feed at {Host} did not contain a calendar.", HostOf(url));
                return null;
            }

            var _Occurrences = _Calendar.GetOccurrences(windowStartUTC, windowEndUTC)
                .Select(ToOccurrence)
                .Where(o => o != null)
                .Select(o => o!)
                .OrderBy(o => o.StartDate)
                .ThenBy(o => o.StartTime)
                .ToList();

            return new CalendarFeed(_Calendar.Properties.Get<string>("X-WR-CALNAME"), _Occurrences);
        }
        catch (Exception _Exception)
        {
            // Ical.Net throws a variety of its own and BCL exceptions on a malformed feed; none of
            // them should reach the family as anything but "could not be read".
            logger.LogWarning(_Exception, "The feed at {Host} could not be parsed.", HostOf(url));
            return null;
        }
    }

    private async Task<string?> FetchAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            using var _Response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (!_Response.IsSuccessStatusCode)
            {
                logger.LogWarning("The feed at {Host} returned {StatusCode}.", HostOf(url), _Response.StatusCode);
                return null;
            }

            if (_Response.Content.Headers.ContentLength > CalendarFeedValues.MaximumFeedBytes)
            {
                logger.LogWarning("The feed at {Host} is {Length} bytes, over the limit.", HostOf(url), _Response.Content.Headers.ContentLength);
                return null;
            }

            var _Text = await _Response.Content.ReadAsStringAsync(cancellationToken);

            return _Text.Length > CalendarFeedValues.MaximumFeedBytes ? null : _Text;
        }
        catch (Exception _Exception) when (_Exception is HttpRequestException or TaskCanceledException or UriFormatException or InvalidOperationException)
        {
            logger.LogWarning(_Exception, "Could not reach the feed at {Host}.", HostOf(url));
            return null;
        }
    }

    private static string HostOf(string url)
        => Uri.TryCreate(url, UriKind.Absolute, out var _Uri) ? _Uri.Host : "(invalid address)";

    /// <summary>
    /// An all-day DTEND is exclusive in iCalendar, so a one-day event ends on the day after it
    /// starts; Home stores the last day inclusive.
    /// </summary>
    private static CalendarFeedOccurrence? ToOccurrence(Occurrence occurrence)
    {
        if (occurrence.Source is not CalendarEvent _Event)
            return null;

        var _Start = occurrence.Period.StartTime;
        var _End = occurrence.Period.EndTime ?? _Start;
        var _IsAllDay = _Event.IsAllDay || !_Start.HasTime;

        if (_IsAllDay)
        {
            var _StartDate = DateOnly.FromDateTime(_Start.Value);
            var _EndDate = DateOnly.FromDateTime(_End.Value);

            if (_EndDate > _StartDate)
                _EndDate = _EndDate.AddDays(-1);

            return new CalendarFeedOccurrence(_Event.Uid ?? string.Empty, _Event.Summary ?? string.Empty, _Event.Location, true, _StartDate, _EndDate, null, null);
        }

        var _StartUTC = _Start.AsUtc;
        var _EndUTC = _End.AsUtc < _StartUTC ? _StartUTC : _End.AsUtc;

        return new CalendarFeedOccurrence(
            _Event.Uid ?? string.Empty,
            _Event.Summary ?? string.Empty,
            _Event.Location,
            false,
            DateOnly.FromDateTime(_StartUTC),
            DateOnly.FromDateTime(_EndUTC),
            TimeOnly.FromDateTime(_StartUTC),
            TimeOnly.FromDateTime(_EndUTC));
    }

    #endregion Methods

}
