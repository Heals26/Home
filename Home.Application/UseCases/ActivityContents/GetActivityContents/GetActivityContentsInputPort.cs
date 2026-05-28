using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ActivityContents.GetActivityContents;

public record GetActivityContentsInputPort(long ActivityRegionID) : IInputPort<IGetActivityContentsOutputPort>;
