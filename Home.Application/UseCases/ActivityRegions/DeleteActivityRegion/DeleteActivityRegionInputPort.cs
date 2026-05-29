using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ActivityRegions.DeleteActivityRegion;

public record DeleteActivityRegionInputPort(long ActivityRegionID) : IInputPort<IDeleteActivityRegionOutputPort>;
