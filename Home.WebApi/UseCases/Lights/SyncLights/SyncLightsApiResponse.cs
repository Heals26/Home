namespace Home.WebApi.UseCases.Lights.SyncLights;

public class SyncLightsApiResponse
{

    #region Properties

    /// <summary>
    /// Bulbs discovered for the first time.
    /// </summary>
    public int Added { get; set; }

    /// <summary>
    /// Bulbs no longer on the provider account, dropped from Home.
    /// </summary>
    public int Removed { get; set; }

    /// <summary>
    /// Bulbs already known, whose name and state were refreshed.
    /// </summary>
    public int Updated { get; set; }

    #endregion Properties

}
