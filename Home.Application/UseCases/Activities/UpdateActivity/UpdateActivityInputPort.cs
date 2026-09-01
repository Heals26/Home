using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.Activities.UpdateActivity;

public record UpdateActivityInputPort(
    long ActivityID,
    PropertyChangeTracker<string> Title,
    PropertyChangeTracker<DateTime?> DueDateUTC,
    PropertyChangeTracker<TimeSpan?> DueTime,
    PropertyChangeTracker<DateTime?> CompletedDateUTC,
    PropertyChangeTracker<int> Sequence,
    PropertyChangeTracker<long?> StateID,
    PropertyChangeTracker<long?> UserID)
    : IInputPort<IUpdateActivityOutputPort>;
