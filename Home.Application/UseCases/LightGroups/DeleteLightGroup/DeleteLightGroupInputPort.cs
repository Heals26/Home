using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.LightGroups.DeleteLightGroup;

public record DeleteLightGroupInputPort(long LightGroupID) : IInputPort<IDeleteLightGroupOutputPort>;
