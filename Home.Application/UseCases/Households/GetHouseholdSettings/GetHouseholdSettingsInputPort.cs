using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Households.GetHouseholdSettings;

public record GetHouseholdSettingsInputPort : IInputPort<IGetHouseholdSettingsOutputPort>;
