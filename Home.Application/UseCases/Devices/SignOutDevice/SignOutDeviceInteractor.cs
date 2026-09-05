using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Devices.SignOutDevice;

/// <summary>
/// Ends one session from somewhere else. The row is deleted rather than expired, because a row
/// nobody can sign in with is only litter and the table already has plenty.
/// </summary>
internal class SignOutDeviceInteractor : IInteractor<SignOutDeviceInputPort, ISignOutDeviceOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        SignOutDeviceInputPort inputPort,
        ISignOutDeviceOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();
        var _CurrentSessionID = _AuthorisationService.GetAuthenticationMetadata().AuthenticationMetadataID;

        var _Device = _PersistenceContext.GetEntities<UserAuthentication>()
            .Where(a => a.AuthenticationMetadataID == inputPort.AuthenticationMetadataID
                && a.User.Household.HouseholdID == _Household.HouseholdID)
            .SingleOrDefault();

        if (_Device == null)
        {
            await outputPort.PresentDeviceNotFoundAsync(inputPort.AuthenticationMetadataID, cancellationToken);
        }
        else if (_CurrentSessionID == inputPort.AuthenticationMetadataID)
        {
            await outputPort.PresentCannotSignOutThisDeviceAsync(cancellationToken);
        }
        else
        {
            _PersistenceContext.Remove(_Device);
            _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

            await outputPort.PresentDeviceSignedOutAsync(cancellationToken);
        }
    }

    #endregion Methods

}
