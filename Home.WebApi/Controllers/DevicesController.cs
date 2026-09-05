using Home.Application.UseCases.Devices.GetDevices;
using Home.Application.UseCases.Devices.SignOutDevice;
using Home.Application.UseCases.Devices.SignOutOtherDevices;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.Devices.GetDevices;
using Home.WebApi.Presenters.Devices.SignOutDevice;
using Home.WebApi.Presenters.Devices.SignOutOtherDevices;
using Home.WebApi.UseCases.Devices.GetDevices;
using Home.WebApi.UseCases.Devices.SignOutOtherDevices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class DevicesController : BaseController
{

    #region Methods

    [HttpGet]
    [ProducesResponseType<GetDevicesApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDevices(
        [FromServices] GetDevicesPresenter presenter,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetDevicesInputPort(), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpDelete("others")]
    [ProducesResponseType<SignOutOtherDevicesApiResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SignOutOtherDevices(
        [FromServices] SignOutOtherDevicesPresenter presenter,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new SignOutOtherDevicesInputPort(), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpDelete("{authenticationMetadataID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SignOutDevice(
        [FromServices] SignOutDevicePresenter presenter,
        [FromRoute] long authenticationMetadataID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new SignOutDeviceInputPort(authenticationMetadataID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
