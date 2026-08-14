using Home.Application.UseCases.LightSchedules.CreateLightSchedule;
using Home.Application.UseCases.LightSchedules.DeleteLightSchedule;
using Home.Application.UseCases.LightSchedules.GetLightSchedules;
using Home.Application.UseCases.LightSchedules.UpdateLightSchedule;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.LightSchedules.CreateLightSchedule;
using Home.WebApi.Presenters.LightSchedules.DeleteLightSchedule;
using Home.WebApi.Presenters.LightSchedules.GetLightSchedules;
using Home.WebApi.Presenters.LightSchedules.UpdateLightSchedule;
using Home.WebApi.UseCases.LightSchedules.CreateLightSchedule;
using Home.WebApi.UseCases.LightSchedules.GetLightSchedules;
using Home.WebApi.UseCases.LightSchedules.UpdateLightSchedule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class LightSchedulesController : BaseController
{

    #region Methods

    [HttpPost]
    [ProducesResponseType<CreateLightScheduleApiResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateLightSchedule(
        [FromServices] CreateLightSchedulePresenter presenter,
        [FromBody] CreateLightScheduleApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new CreateLightScheduleInputPort(request.Name, request.LightSceneID, request.Trigger, request.TimeOfDay, request.OffsetMinutes, request.DaysOfWeek), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpDelete("{lightScheduleID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLightSchedule(
        [FromServices] DeleteLightSchedulePresenter presenter,
        [FromRoute] long lightScheduleID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new DeleteLightScheduleInputPort(lightScheduleID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpGet]
    [ProducesResponseType<GetLightSchedulesApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLightSchedules(
        [FromServices] GetLightSchedulesPresenter presenter,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetLightSchedulesInputPort(), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPatch("{lightScheduleID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLightSchedule(
        [FromServices] UpdateLightSchedulePresenter presenter,
        [FromRoute] long lightScheduleID,
        [FromBody] UpdateLightScheduleApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new UpdateLightScheduleInputPort(lightScheduleID, request.Name, request.IsEnabled, request.TimeOfDay, request.DaysOfWeek), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
