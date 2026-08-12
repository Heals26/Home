using Home.Application.UseCases.LightScenes.ApplyLightScene;
using Home.Application.UseCases.LightScenes.CaptureLightScene;
using Home.Application.UseCases.LightScenes.DeleteLightScene;
using Home.Application.UseCases.LightScenes.GetLightScenes;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.LightScenes.ApplyLightScene;
using Home.WebApi.Presenters.LightScenes.CaptureLightScene;
using Home.WebApi.Presenters.LightScenes.DeleteLightScene;
using Home.WebApi.Presenters.LightScenes.GetLightScenes;
using Home.WebApi.UseCases.LightScenes.CaptureLightScene;
using Home.WebApi.UseCases.LightScenes.GetLightScenes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class LightScenesController : BaseController
{

    #region Methods

    [HttpPost("{lightSceneID}/apply")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ApplyLightScene(
        [FromServices] ApplyLightScenePresenter presenter,
        [FromRoute] long lightSceneID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new ApplyLightSceneInputPort(lightSceneID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPost]
    [ProducesResponseType<CaptureLightSceneApiResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CaptureLightScene(
        [FromServices] CaptureLightScenePresenter presenter,
        [FromBody] CaptureLightSceneApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new CaptureLightSceneInputPort(request.Name, request.LightGroupID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpDelete("{lightSceneID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLightScene(
        [FromServices] DeleteLightScenePresenter presenter,
        [FromRoute] long lightSceneID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new DeleteLightSceneInputPort(lightSceneID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpGet]
    [ProducesResponseType<GetLightScenesApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLightScenes(
        [FromServices] GetLightScenesPresenter presenter,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetLightScenesInputPort(), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
