using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ActivityRegions.CreateActivityRegion;

public record CreateActivityRegionInputPort(
    long ActivityID,
    long CardSectionID)
    : IInputPort<ICreateActivityRegionOutputPort>;