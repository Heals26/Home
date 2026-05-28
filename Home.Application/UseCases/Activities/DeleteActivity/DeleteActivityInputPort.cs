using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Activities.DeleteActivity;

public record DeleteActivityInputPort(long ActivityID) : IInputPort<IDeleteActivityOutputPort>;
