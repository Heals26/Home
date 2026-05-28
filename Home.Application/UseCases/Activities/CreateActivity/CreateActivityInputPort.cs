using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Activities.CreateActivity;

public record CreateActivityInputPort(
    string Title,
    DateTime? DueDateUTC,
    long? StateID,
    long? StatusID,
    long? UserID)
    : IInputPort<ICreateActivityOutputPort>;
