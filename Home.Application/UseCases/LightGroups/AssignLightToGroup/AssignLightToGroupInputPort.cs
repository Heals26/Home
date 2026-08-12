using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.LightGroups.AssignLightToGroup;

public record AssignLightToGroupInputPort(string LightID, long LightGroupID)
    : IInputPort<IAssignLightToGroupOutputPort>;
