using Home.Application.UseCases.Lights.GetLights;
using Home.Application.UseCases.Lights.SetLightState;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.Lights.GetLights;
using Home.WebApi.Presenters.Lights.SetLightState;
using Home.WebApi.UseCases.Lights.GetLights;
using Home.WebApi.UseCases.Lights.SetLightState;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class LightsController : BaseController
{

    #region Methods

    [HttpGet]
    [ProducesResponseType<GetLightsApiResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetLights(
        [FromServices] GetLightsPresenter presenter,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetLightsInputPort(), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPatch("{lightID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> SetLightState(
        [FromServices] SetLightStatePresenter presenter,
        [FromRoute] string lightID,
        [FromBody] SetLightStateApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new SetLightStateInputPort(lightID, request.IsOn, request.Brightness, request.Hue, request.Saturation, request.Kelvin), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
