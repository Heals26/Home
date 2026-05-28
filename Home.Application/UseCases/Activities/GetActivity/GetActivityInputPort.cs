using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Activities.GetActivity;

public record GetActivityInputPort(long ActivityID) : IInputPort<IGetActivityOutputPort>;
