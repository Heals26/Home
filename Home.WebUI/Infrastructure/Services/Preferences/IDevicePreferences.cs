namespace Home.WebUI.Infrastructure.Services.Preferences;

/// <summary>
/// Per-device choices kept in the browser: the scope the board shows, and the like. Nothing here
/// is shared between devices or reaches the API, which is the point: a kitchen tablet showing
/// everyone and a phone showing "just me" are both right.
/// </summary>
public interface IDevicePreferences
{

    #region Methods

    /// <summary>
    /// The stored value, or an empty string when nothing is stored or storage cannot be read.
    /// </summary>
    Task<string> GetAsync(string key, CancellationToken cancellationToken);

    Task SetAsync(string key, string value, CancellationToken cancellationToken);

    #endregion Methods

}
