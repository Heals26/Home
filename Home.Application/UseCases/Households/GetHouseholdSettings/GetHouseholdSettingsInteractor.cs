using CleanArchitecture.Mediator;
using Home.Application.Services.Security;

namespace Home.Application.UseCases.Households.GetHouseholdSettings;

internal class GetHouseholdSettingsInteractor
    : IInteractor<GetHouseholdSettingsInputPort, IGetHouseholdSettingsOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetHouseholdSettingsInputPort inputPort,
        IGetHouseholdSettingsOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        await outputPort.PresentHouseholdSettingsAsync(_AuthorisationService.GetHousehold(), cancellationToken);
    }

    #endregion Methods

}
