using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;

namespace Home.Application.UseCases.Households.UpdateHouseholdSettings;

internal class UpdateHouseholdSettingsInteractor
    : IInteractor<UpdateHouseholdSettingsInputPort, IUpdateHouseholdSettingsOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        UpdateHouseholdSettingsInputPort inputPort,
        IUpdateHouseholdSettingsOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        if (inputPort.Latitude.HasBeenSet)
            _Household.Latitude = inputPort.Latitude.Value;

        // An empty token is a deliberate disconnect and stores as null.
        if (inputPort.LifxApiToken.HasBeenSet)
            _Household.LifxApiToken = string.IsNullOrWhiteSpace(inputPort.LifxApiToken.Value)
                ? null
                : inputPort.LifxApiToken.Value.Trim();

        if (inputPort.Longitude.HasBeenSet)
            _Household.Longitude = inputPort.Longitude.Value;

        if (inputPort.Name.HasBeenSet)
            _Household.Name = inputPort.Name.Value.Trim();

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentHouseholdSettingsUpdatedAsync(cancellationToken);
    }

    #endregion Methods

}
