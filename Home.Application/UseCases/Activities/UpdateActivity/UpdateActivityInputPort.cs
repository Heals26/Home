using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.Activities.UpdateActivity;

public record UpdateActivityInputPort(
    long ActivityID,
    PropertyChangeTracker<string> Title,
    PropertyChangeTracker<DateTime?> DueDateUTC,
    PropertyChangeTracker<TimeSpan?> DueTime,
    PropertyChangeTracker<DateTime?> CompletedDateUTC,
    PropertyChangeTracker<long?> StateID,
    PropertyChangeTracker<long?> StatusID,
    PropertyChangeTracker<long?> UserID)
    : IInputPort<IUpdateActivityOutputPort>;
