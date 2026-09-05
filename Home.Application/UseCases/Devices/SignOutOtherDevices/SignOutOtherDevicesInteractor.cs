using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Devices.SignOutOtherDevices;

/// <summary>
/// Ends every session in the household except the one asking.
/// <para>
/// A household accumulates one session per sign-in and nothing prunes them, so the devices screen
/// can show dozens of rows nobody recognises. Ending them one at a time is not a remedy anybody
/// would use. Expired rows go too, which is the only thing in the application that tidies them.
/// </para>
/// </summary>
internal class SignOutOtherDevicesInteractor : IInteractor<SignOutOtherDevicesInputPort, ISignOutOtherDevicesOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        SignOutOtherDevicesInputPort inputPort,
        ISignOutOtherDevicesOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();
        var _CurrentSessionID = _AuthorisationService.GetAuthenticationMetadata().AuthenticationMetadataID;

        if (_CurrentSessionID == null)
        {
            await outputPort.PresentCurrentDeviceUnknownAsync(cancellationToken);
        }
        else
        {
            var _Others = _PersistenceContext.GetEntities<UserAuthentication>()
                .Where(a => a.User.Household.HouseholdID == _Household.HouseholdID
                    && a.AuthenticationMetadataID != _CurrentSessionID.Value)
                .ToList();

            _PersistenceContext.RemoveRange(_Others);
            _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

            await outputPort.PresentOtherDevicesSignedOutAsync(_Others.Count, cancellationToken);
        }
    }

    #endregion Methods

}
