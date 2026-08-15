using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Tags.DeleteTag;

public record DeleteTagInputPort(long TagID) : IInputPort<IDeleteTagOutputPort>;
