using Home.Application.UseCases.LightGroups.AssignLightToGroup;
using Home.Application.UseCases.LightGroups.CreateLightGroup;
using Home.Application.UseCases.LightGroups.DeleteLightGroup;
using Home.Application.UseCases.LightGroups.SetLightGroupState;
using Home.Application.UseCases.LightGroups.UpdateLightGroup;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.LightGroups.AssignLightToGroup;
using Home.WebApi.Presenters.LightGroups.CreateLightGroup;
using Home.WebApi.Presenters.LightGroups.DeleteLightGroup;
using Home.WebApi.Presenters.LightGroups.SetLightGroupState;
using Home.WebApi.Presenters.LightGroups.UpdateLightGroup;
using Home.WebApi.UseCases.LightGroups.AssignLightToGroup;
using Home.WebApi.UseCases.LightGroups.CreateLightGroup;
using Home.WebApi.UseCases.LightGroups.SetLightGroupState;
using Home.WebApi.UseCases.LightGroups.UpdateLightGroup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class LightGroupsController : BaseController
{

    #region Methods

    [HttpPut("{lightGroupID}/lights")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignLightToGroup(
        [FromServices] AssignLightToGroupPresenter presenter,
        [FromRoute] long lightGroupID,
        [FromBody] AssignLightToGroupApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new AssignLightToGroupInputPort(request.LightID, lightGroupID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPost]
    [ProducesResponseType<CreateLightGroupApiResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateLightGroup(
        [FromServices] CreateLightGroupPresenter presenter,
        [FromBody] CreateLightGroupApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new CreateLightGroupInputPort(request.Name), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpDelete("{lightGroupID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteLightGroup(
        [FromServices] DeleteLightGroupPresenter presenter,
        [FromRoute] long lightGroupID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new DeleteLightGroupInputPort(lightGroupID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPatch("{lightGroupID}/state")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> SetLightGroupState(
        [FromServices] SetLightGroupStatePresenter presenter,
        [FromRoute] long lightGroupID,
        [FromBody] SetLightGroupStateApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new SetLightGroupStateInputPort(lightGroupID, request.IsOn, request.Brightness, request.Hue, request.Saturation, request.Kelvin), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPatch("{lightGroupID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLightGroup(
        [FromServices] UpdateLightGroupPresenter presenter,
        [FromRoute] long lightGroupID,
        [FromBody] UpdateLightGroupApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new UpdateLightGroupInputPort(lightGroupID, request.Name, request.Sequence), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
