using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.Users.UpdateUser;

public record UpdateUserInputPort(
    PropertyChangeTracker<string> Email,
    PropertyChangeTracker<string> FirstName,
    PropertyChangeTracker<string> LastName,
    PropertyChangeTracker<string> MiddleNames,
    PropertyChangeTracker<string> Password,
    long UserID)
    : IInputPort<IUpdateUserOutputPort>;