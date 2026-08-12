namespace Home.WebApi.Infrastructure.Lights;

internal static class LightValues
{

    #region Constants

    /// <summary>
    /// The LIFX cloud API root. Trailing slash matters — relative request URIs are resolved
    /// against it, and without it the "v1" segment is dropped.
    /// </summary>
    public const string LifxBaseUrl = "https://api.lifx.com/v1/";

    /// <summary>
    /// LIFX combines at most 25 comma-separated selectors in a single request, which is what lets
    /// a whole room be set in one call instead of one per bulb.
    /// </summary>
    public const int MaxSelectorsPerRequest = 25;

    /// <summary>
    /// Short by HTTP standards, because a wall tablet showing a spinner for 100 seconds is worse
    /// than one saying the lights are unreachable.
    /// </summary>
    public const int RequestTimeoutSeconds = 8;

    #endregion Constants

}
