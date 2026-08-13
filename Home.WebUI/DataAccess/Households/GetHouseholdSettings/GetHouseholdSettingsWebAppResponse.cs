namespace Home.WebUI.DataAccess.Households.GetHouseholdSettings;

public class GetHouseholdSettingsWebAppResponse
{

    #region Properties

    /// <summary>
    /// Whether a LIFX API token is stored. The token itself never leaves the server.
    /// </summary>
    public bool HasLifxApiToken { get; set; }

    /// <summary>
    /// The household's latitude in decimal degrees, used for future sunrise and
    /// sunset triggers. Null when the household hasn't set a location.
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// The household's longitude in decimal degrees.
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// The household's display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    #endregion Properties

}
