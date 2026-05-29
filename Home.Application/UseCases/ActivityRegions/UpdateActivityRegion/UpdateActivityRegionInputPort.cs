using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.ActivityRegions.UpdateActivityRegion;

public record UpdateActivityRegionInputPort(
    long ActivityRegionID,
    PropertyChangeTracker<string> Region,
    PropertyChangeTracker<int> Sequence)
    : IInputPort<IUpdateActivityRegionOutputPort>;
