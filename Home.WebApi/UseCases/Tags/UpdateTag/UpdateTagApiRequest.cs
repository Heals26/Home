using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.WebApi.UseCases.Tags.UpdateTag;

/// <summary>
/// Omit a property to leave it alone. Colour, when sent, must be in the form #RRGGBB.
/// </summary>
public record UpdateTagApiRequest(
    PropertyChangeTracker<string> Colour,
    PropertyChangeTracker<string> Name);
