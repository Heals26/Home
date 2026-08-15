using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.WebApi.UseCases.ActivityStates.UpdateActivityState;

/// <summary>
/// Omit a property to leave it alone. Sending only Sequence is how a column is reordered.
/// </summary>
public record UpdateActivityStateApiRequest(
    PropertyChangeTracker<bool> IsComplete,
    PropertyChangeTracker<string> Name,
    PropertyChangeTracker<int> Sequence);
