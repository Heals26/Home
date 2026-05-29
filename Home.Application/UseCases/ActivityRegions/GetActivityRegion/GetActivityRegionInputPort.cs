using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ActivityRegions.GetActivityRegion;

public record GetActivityRegionInputPort(long ActivityRegionID) : IInputPort<IGetActivityRegionOutputPort>;
