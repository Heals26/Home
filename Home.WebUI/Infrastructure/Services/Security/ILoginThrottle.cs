namespace Home.WebUI.Infrastructure.Services.Security;

public interface ILoginThrottle
{

    #region Methods

    /// <summary>
    /// Null when the account may be tried, otherwise how long until it may be tried again.
    /// </summary>
    TimeSpan? GetLockout(string username);

    void RecordFailure(string username);

    void RecordSuccess(string username);

    #endregion Methods

}
