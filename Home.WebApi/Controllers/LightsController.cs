using Home.Application.UseCases.Lights.GetLights;
using Home.Application.UseCases.Lights.SetLightState;
using Home.Application.UseCases.Lights.StartLightEffect;
using Home.Application.UseCases.Lights.SyncLights;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.Lights.GetLights;
using Home.WebApi.Presenters.Lights.SetLightState;
using Home.WebApi.Presenters.Lights.StartLightEffect;
using Home.WebApi.Presenters.Lights.SyncLights;
using Home.WebApi.UseCases.Lights.GetLights;
using Home.WebApi.UseCases.Lights.SetLightState;
using Home.WebApi.UseCases.Lights.StartLightEffect;
using Home.WebApi.UseCases.Lights.SyncLights;
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

    [HttpPost("effects")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> StartLightEffect(
        [FromServices] StartLightEffectPresenter presenter,
        [FromBody] StartLightEffectApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new StartLightEffectInputPort(request.LightGroupID, request.Kind, request.Hue, request.Saturation, request.PeriodSeconds, request.Cycles), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPost("sync")]
    [ProducesResponseType<SyncLightsApiResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> SyncLights(
        [FromServices] SyncLightsPresenter presenter,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new SyncLightsInputPort(), presenter, this.ServiceFactory, cancellationToken);

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
