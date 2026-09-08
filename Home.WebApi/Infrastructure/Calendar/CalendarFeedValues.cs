namespace Home.WebApi.Infrastructure.Calendar;

public static class CalendarFeedValues
{

    #region Fields

    /// <summary>
    /// A feed larger than this is not a family calendar. Refusing it keeps one bad link from
    /// filling memory on a small home server.
    /// </summary>
    public static readonly int MaximumFeedBytes = 5 * 1024 * 1024;

    public static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(30);

    public static readonly int RequestTimeoutSeconds = 20;

    #endregion Fields

}
