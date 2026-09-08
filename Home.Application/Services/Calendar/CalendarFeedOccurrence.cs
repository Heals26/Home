namespace Home.Application.Services.Calendar;

/// <summary>
/// One occurrence read from a feed, already expanded from whatever rule produced it. For a timed
/// occurrence the dates and times are UTC wall-clock, so storing them with zone "UTC" is exact.
/// For an all-day occurrence they are plain dates and the times are null.
/// </summary>
public record CalendarFeedOccurrence(
    string ExternalUID,
    string Title,
    string? Location,
    bool IsAllDay,
    DateOnly StartDate,
    DateOnly EndDate,
    TimeOnly? StartTime,
    TimeOnly? EndTime);
