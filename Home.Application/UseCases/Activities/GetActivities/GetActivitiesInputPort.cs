using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Activities.GetActivities;

public record GetActivitiesInputPort() : IInputPort<IGetActivitiesOutputPort>;
