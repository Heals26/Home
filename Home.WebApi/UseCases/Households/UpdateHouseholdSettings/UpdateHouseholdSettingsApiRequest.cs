using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.WebApi.UseCases.Households.UpdateHouseholdSettings;

/// <summary>
/// Omit a property to leave it alone, so saving one settings section can't clobber another.
/// An empty LifxApiToken disconnects; the token is never returned by any endpoint.
/// </summary>
public record UpdateHouseholdSettingsApiRequest(
    PropertyChangeTracker<double?> Latitude,
    PropertyChangeTracker<string> LifxApiToken,
    PropertyChangeTracker<double?> Longitude,
    PropertyChangeTracker<string> Name);
