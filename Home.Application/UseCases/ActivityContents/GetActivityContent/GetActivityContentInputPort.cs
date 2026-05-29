using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ActivityContents.GetActivityContent;

public record GetActivityContentInputPort(long ActivityContentID) : IInputPort<IGetActivityContentOutputPort>;
