using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ActivityRegions.CreateActivityRegion;

public record CreateActivityRegionInputPort(
    long ActivityID,
    string Region)
    : IInputPort<ICreateActivityRegionOutputPort>;
