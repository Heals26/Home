namespace Home.WebApi.UseCases.CalendarSubscriptions.Models;

public class CalendarSubscriptionDto
{

    #region Properties

    public long CalendarSubscriptionID { get; set; }

    /// <summary>
    /// The feed's host, never its full address: the secret address is a credential.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Why the last fetch failed, or null when it succeeded.
    /// </summary>
    public string LastError { get; set; }

    public DateTime? LastFetchedUTC { get; set; }
    public string Name { get; set; } = string.Empty;

    #endregion Properties

}
