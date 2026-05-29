using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.WebApi.UseCases.Activities.UpdateActivity;

public record UpdateActivityApiRequest(
    PropertyChangeTracker<string> Title,
    PropertyChangeTracker<DateTime?> DueDateUTC,
    PropertyChangeTracker<DateTime?> CompletedDateUTC,
    PropertyChangeTracker<long?> StateID,
    PropertyChangeTracker<long?> StatusID,
    PropertyChangeTracker<long?> UserID);
