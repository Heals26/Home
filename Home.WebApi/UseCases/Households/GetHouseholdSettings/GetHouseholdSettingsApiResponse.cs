namespace Home.WebApi.UseCases.Households.GetHouseholdSettings;

public class GetHouseholdSettingsApiResponse
{

    #region Properties

    /// <summary>
    /// Whether a LIFX API token is stored. The token itself never leaves the server.
    /// </summary>
    public bool HasLifxApiToken { get; set; }

    /// <summary>
    /// The caller's own household — used by clients to scope live change notifications.
    /// </summary>
    public long HouseholdID { get; set; }

    /// <summary>
    /// Decimal degrees, or null when the household hasn't set a location.
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Decimal degrees, or null when the household hasn't set a location.
    /// </summary>
    public double? Longitude { get; set; }

    public string Name { get; set; } = string.Empty;

    #endregion Properties

}
