using AutoMapper;
using Home.Application.UseCases.Devices.SignOutOtherDevices;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Devices.SignOutOtherDevices;

namespace Home.WebApi.Presenters.Devices.SignOutOtherDevices;

public class SignOutOtherDevicesPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ISignOutOtherDevicesOutputPort
{

    #region Methods

    Task ISignOutOtherDevicesOutputPort.PresentCurrentDeviceUnknownAsync(CancellationToken cancellationToken)
        => this.ConflictAsync(cancellationToken);

    Task ISignOutOtherDevicesOutputPort.PresentOtherDevicesSignedOutAsync(int signedOutCount, CancellationToken cancellationToken)
        => this.OkAsync(new SignOutOtherDevicesApiResponse() { SignedOutCount = signedOutCount }, cancellationToken);

    #endregion Methods

}
