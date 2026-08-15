using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Tags.CreateTag;

public record CreateTagInputPort(
    string Colour,
    string Name)
    : IInputPort<ICreateTagOutputPort>;
