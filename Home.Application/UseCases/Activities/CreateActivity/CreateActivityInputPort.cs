using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Activities.CreateActivity;

public record CreateActivityInputPort(
    string Title,
    DateTime? DueDateUTC,
    TimeSpan? DueTime,
    long? StateID,
    long? UserID)
    : IInputPort<ICreateActivityOutputPort>;
