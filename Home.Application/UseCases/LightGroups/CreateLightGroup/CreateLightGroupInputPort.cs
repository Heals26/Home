using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.LightGroups.CreateLightGroup;

public record CreateLightGroupInputPort(string Name) : IInputPort<ICreateLightGroupOutputPort>;
