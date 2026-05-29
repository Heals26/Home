using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ActivityContents.DeleteActivityContent;

public record DeleteActivityContentInputPort(long ActivityContentID) : IInputPort<IDeleteActivityContentOutputPort>;
