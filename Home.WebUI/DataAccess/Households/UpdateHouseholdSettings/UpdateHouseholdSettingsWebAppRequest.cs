using Home.WebUI.Infrastructure.ChangeTrackers;

namespace Home.WebUI.DataAccess.Households.UpdateHouseholdSettings;

/// <summary>
/// Omit a property to leave it alone, so saving one section can't clobber another.
/// </summary>
public class UpdateHouseholdSettingsWebAppRequest
{

    #region Properties

    /// <summary>
    /// Decimal degrees, -90 to 90. Send null to clear the location.
    /// </summary>
    public PropertyChangeTracker<double?> Latitude { get; set; }

    /// <summary>
    /// The LIFX API token. Send an empty string to disconnect.
    /// </summary>
    public PropertyChangeTracker<string> LifxApiToken { get; set; }

    /// <summary>
    /// Decimal degrees, -180 to 180. Send null to clear the location.
    /// </summary>
    public PropertyChangeTracker<double?> Longitude { get; set; }

    /// <summary>
    /// The household's display name.
    /// </summary>
    public PropertyChangeTracker<string> Name { get; set; }

    #endregion Properties

}
