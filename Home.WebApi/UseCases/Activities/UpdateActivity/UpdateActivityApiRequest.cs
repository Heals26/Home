using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.WebApi.UseCases.Activities.UpdateActivity;

/// <summary>
/// Omit a property to leave it alone. Moving a card into a completed column stamps
/// CompletedDateUTC on its own; sending it as well only overrides the date used.
/// </summary>
public record UpdateActivityApiRequest(
    PropertyChangeTracker<string> Title,
    PropertyChangeTracker<DateTime?> DueDateUTC,
    PropertyChangeTracker<TimeSpan?> DueTime,
    PropertyChangeTracker<DateTime?> CompletedDateUTC,
    PropertyChangeTracker<long?> StateID,
    PropertyChangeTracker<long?> UserID);
