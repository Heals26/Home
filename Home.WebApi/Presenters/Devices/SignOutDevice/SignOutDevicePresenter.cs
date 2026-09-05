using AutoMapper;
using Home.Application.UseCases.Devices.SignOutDevice;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.Devices.SignOutDevice;

public class SignOutDevicePresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ISignOutDeviceOutputPort
{

    #region Methods

    /// <summary>
    /// A 409 rather than a 404: the session exists, it simply cannot be ended from here.
    /// </summary>
    Task ISignOutDeviceOutputPort.PresentCannotSignOutThisDeviceAsync(CancellationToken cancellationToken)
        => this.ConflictAsync(cancellationToken);

    Task ISignOutDeviceOutputPort.PresentDeviceNotFoundAsync(long authenticationMetadataID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Device {authenticationMetadataID} Not Found", cancellationToken);

    Task ISignOutDeviceOutputPort.PresentDeviceSignedOutAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    #endregion Methods

}
