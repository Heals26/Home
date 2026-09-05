namespace Home.Application.Infrastructure.Security;

/// <summary>
/// Turns the User-Agent a browser sends into something a family would recognise on a list of
/// signed-in devices.
/// <para>
/// A raw User-Agent is unreadable and half of it is a lie kept for compatibility, so this reads
/// only the two parts anyone recognises: the browser and the machine it is on. Nothing here is a
/// security decision, and nothing downstream may treat it as one: a caller can send whatever
/// User-Agent it likes. It is a label, not evidence.
/// </para>
/// </summary>
public static class DeviceLabelLogic
{

    #region Constants

    /// <summary>
    /// What to call a device that sent nothing useful, rather than showing an empty row.
    /// </summary>
    public const string UnknownDevice = "Unknown device";

    /// <summary>
    /// Longer than any label this produces, and short enough to keep a hostile User-Agent out of
    /// the column. The column itself allows 200.
    /// </summary>
    private const int MaximumLength = 100;

    #endregion Constants

    #region Methods

    /// <summary>
    /// A short, human name for the device behind a User-Agent, never null and never empty.
    /// </summary>
    public static string Describe(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
            return UnknownDevice;

        var _Browser = DescribeBrowser(userAgent);
        var _Platform = DescribePlatform(userAgent);

        var _Label = (_Browser, _Platform) switch
        {
            (null, null) => UnknownDevice,
            (not null, null) => _Browser,
            (null, not null) => _Platform,
            _ => $"{_Browser} on {_Platform}"
        };

        return _Label.Length > MaximumLength ? _Label[..MaximumLength] : _Label;
    }

    /// <summary>
    /// Order matters throughout. Every Chromium browser still claims to be Chrome and Safari, and
    /// Chrome still claims to be Safari, so the most specific claim has to be tested first.
    /// </summary>
    private static string? DescribeBrowser(string userAgent)
    {
        if (Contains(userAgent, "Edg/") || Contains(userAgent, "Edge/"))
            return "Edge";

        if (Contains(userAgent, "OPR/") || Contains(userAgent, "Opera"))
            return "Opera";

        if (Contains(userAgent, "SamsungBrowser"))
            return "Samsung Internet";

        if (Contains(userAgent, "Firefox") || Contains(userAgent, "FxiOS"))
            return "Firefox";

        if (Contains(userAgent, "CriOS") || Contains(userAgent, "Chrome") || Contains(userAgent, "Chromium"))
            return "Chrome";

        return Contains(userAgent, "Safari") ? "Safari" : null;
    }

    private static string? DescribePlatform(string userAgent)
    {
        if (Contains(userAgent, "iPhone"))
            return "iPhone";

        if (Contains(userAgent, "iPad"))
            return "iPad";

        if (Contains(userAgent, "Android"))
            return "Android";

        if (Contains(userAgent, "Windows"))
            return "Windows";

        // Tested after iPhone and iPad, both of which also say Mac OS X.
        if (Contains(userAgent, "Macintosh") || Contains(userAgent, "Mac OS"))
            return "Mac";

        // Tested after Android, which says Linux too.
        return Contains(userAgent, "Linux") || Contains(userAgent, "X11") ? "Linux" : null;
    }

    private static bool Contains(string userAgent, string token)
        => userAgent.Contains(token, StringComparison.OrdinalIgnoreCase);

    #endregion Methods

}
