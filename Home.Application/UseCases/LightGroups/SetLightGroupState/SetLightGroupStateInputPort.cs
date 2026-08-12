using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.LightGroups.SetLightGroupState;

public record SetLightGroupStateInputPort(
    long LightGroupID,
    PropertyChangeTracker<bool> IsOn,
    PropertyChangeTracker<double> Brightness,
    PropertyChangeTracker<double> Hue,
    PropertyChangeTracker<double> Saturation,
    PropertyChangeTracker<int> Kelvin)
    : IInputPort<ISetLightGroupStateOutputPort>;
