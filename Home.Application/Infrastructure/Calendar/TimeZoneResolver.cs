namespace Home.Application.Infrastructure.Calendar;

/// <summary>
/// Turns the IANA zone name a browser reports into something .NET can convert with. Falls back to
/// UTC rather than throwing, because a zone this machine has never heard of must not take the
/// whole calendar down with it.
/// </summary>
public static class TimeZoneResolver
{

    #region Methods

    public static TimeZoneInfo Resolve(string? timeZoneID)
    {
        if (string.IsNullOrWhiteSpace(timeZoneID))
            return TimeZoneInfo.Utc;

        return TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneID, out var _Zone)
            ? _Zone
            : TimeZoneInfo.Utc;
    }

    public static DateTime ToLocal(DateTime utc, TimeZoneInfo zone)
        => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), zone);

    /// <summary>
    /// A wall-clock moment in the zone, as the UTC instant it names. A time that does not exist on
    /// that day (the spring-forward gap) is nudged forward by the gap rather than rejected.
    /// </summary>
    public static DateTime ToUtc(DateOnly date, TimeOnly time, TimeZoneInfo zone)
    {
        var _Local = DateTime.SpecifyKind(date.ToDateTime(time), DateTimeKind.Unspecified);

        if (zone.IsInvalidTime(_Local))
            _Local = _Local.AddHours(1);

        return TimeZoneInfo.ConvertTimeToUtc(_Local, zone);
    }

    #endregion Methods

}
