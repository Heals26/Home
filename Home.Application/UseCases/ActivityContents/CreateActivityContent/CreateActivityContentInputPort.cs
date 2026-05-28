using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ActivityContents.CreateActivityContent;

public record CreateActivityContentInputPort(
    long ActivityRegionID,
    string Content)
    : IInputPort<ICreateActivityContentOutputPort>;
