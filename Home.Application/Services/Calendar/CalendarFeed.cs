namespace Home.Application.Services.Calendar;

/// <summary>
/// What a feed said when it was read. <paramref name="Name"/> is the calendar's own name when
/// the feed carries one, so a household can leave the name blank and get the right one.
/// </summary>
public record CalendarFeed(string? Name, IReadOnlyList<CalendarFeedOccurrence> Occurrences);
