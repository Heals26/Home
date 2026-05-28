using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Activities.GetAssignedActivities;

public record GetAssignedActivitiesInputPort() : IInputPort<IGetAssignedActivitiesOutputPort>;
