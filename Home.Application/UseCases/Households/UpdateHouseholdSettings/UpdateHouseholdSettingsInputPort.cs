using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.Households.UpdateHouseholdSettings;

public record UpdateHouseholdSettingsInputPort(
    PropertyChangeTracker<double?> Latitude,
    PropertyChangeTracker<string> LifxApiToken,
    PropertyChangeTracker<double?> Longitude,
    PropertyChangeTracker<string> Name) : IInputPort<IUpdateHouseholdSettingsOutputPort>;
