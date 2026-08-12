using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.Lights.SetLightState;

public record SetLightStateInputPort(
    string LightID,
    PropertyChangeTracker<bool> IsOn,
    PropertyChangeTracker<double> Brightness,
    PropertyChangeTracker<double> Hue,
    PropertyChangeTracker<double> Saturation,
    PropertyChangeTracker<int> Kelvin)
    : IInputPort<ISetLightStateOutputPort>;
