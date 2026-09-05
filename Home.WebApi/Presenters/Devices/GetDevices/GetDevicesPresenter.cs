using AutoMapper;
using Home.Application.Infrastructure.Security;
using Home.Application.UseCases.Devices.GetDevices;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Devices.GetDevices;

namespace Home.WebApi.Presenters.Devices.GetDevices;

public class GetDevicesPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetDevicesOutputPort
{

    #region Methods

    Task IGetDevicesOutputPort.PresentDevicesAsync(IEnumerable<UserAuthentication> devices, long? currentSessionID, CancellationToken cancellationToken)
        => this.OkAsync(new GetDevicesApiResponse()
        {
            Devices = [.. devices.Select(d => new DeviceDto()
            {
                AuthenticationMetadataID = d.AuthenticationMetadataID,
                IsCurrentDevice = currentSessionID != null && d.AuthenticationMetadataID == currentSessionID,
                LastUsedOnUTC = d.LastUsedOnUTC,
                // Sessions created before the label was captured have nothing stored, and an empty
                // row reads worse than saying so.
                Name = string.IsNullOrWhiteSpace(d.DeviceLabel) ? DeviceLabelLogic.UnknownDevice : d.DeviceLabel,
                SignedInOnUTC = d.DateSetUTC
            })]
        }, cancellationToken);

    #endregion Methods

}
