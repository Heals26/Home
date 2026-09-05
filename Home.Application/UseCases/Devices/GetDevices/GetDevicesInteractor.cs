using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Devices.GetDevices;

/// <summary>
/// The household's signed-in devices. Expired sessions are left out: a row nobody can sign in with
/// is not a device, and the table keeps them because nothing prunes it.
/// </summary>
internal class GetDevicesInteractor : IInteractor<GetDevicesInputPort, IGetDevicesOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetDevicesInputPort inputPort,
        IGetDevicesOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _TimeProvider = serviceFactory.GetService<TimeProvider>();

        var _Household = _AuthorisationService.GetHousehold();
        var _Now = _TimeProvider.GetUtcNow().UtcDateTime;

        var _Devices = _PersistenceContext.GetEntities<UserAuthentication>()
            .Where(a => a.User.Household.HouseholdID == _Household.HouseholdID
                && a.ExpiresOnUTC > _Now)
            .Select(a => new
            {
                Device = a,
                a.User
            })
            .ToList()
            .Select(a => a.Device)
            // A device in daily use is the one the family is looking for, so most recently used
            // leads. A session never used since sign-in falls back to when it was created.
            .OrderByDescending(a => a.LastUsedOnUTC ?? a.DateSetUTC)
            .ThenByDescending(a => a.AuthenticationMetadataID)
            .ToList();

        await outputPort.PresentDevicesAsync(
            _Devices,
            _AuthorisationService.GetAuthenticationMetadata().AuthenticationMetadataID,
            cancellationToken);
    }

    #endregion Methods

}
