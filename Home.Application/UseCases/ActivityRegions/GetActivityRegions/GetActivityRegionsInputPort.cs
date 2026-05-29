using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ActivityRegions.GetActivityRegions;

public record GetActivityRegionsInputPort(long ActivityID) : IInputPort<IGetActivityRegionsOutputPort>;
