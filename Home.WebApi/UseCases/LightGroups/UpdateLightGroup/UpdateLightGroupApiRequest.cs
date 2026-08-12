using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.WebApi.UseCases.LightGroups.UpdateLightGroup;

/// <summary>
/// Omit a property to leave it alone. Sending only Sequence reorders without renaming.
/// </summary>
public record UpdateLightGroupApiRequest(
    PropertyChangeTracker<string> Name,
    PropertyChangeTracker<int> Sequence);
