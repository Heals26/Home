using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Tags.GetTags;

public record GetTagsInputPort() : IInputPort<IGetTagsOutputPort>;
