using Home.Application.UseCases.Households.GetHouseholdSettings;
using Home.Application.UseCases.Households.GetSetupStatus;
using Home.Application.UseCases.Households.RegisterHousehold;
using Home.Application.UseCases.Households.UpdateHouseholdSettings;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.Households.GetHouseholdSettings;
using Home.WebApi.Presenters.Households.GetSetupStatus;
using Home.WebApi.Presenters.Households.RegisterHousehold;
using Home.WebApi.Presenters.Households.UpdateHouseholdSettings;
using Home.WebApi.UseCases.Households.GetHouseholdSettings;
using Home.WebApi.UseCases.Households.GetSetupStatus;
using Home.WebApi.UseCases.Households.RegisterHousehold;
using Home.WebApi.UseCases.Households.UpdateHouseholdSettings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class HouseholdsController : BaseController
{

    #region Methods

    [AllowAnonymous]
    [HttpGet("setup-status")]
    [ProducesResponseType<GetSetupStatusApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSetupStatus(
        [FromServices] GetSetupStatusPresenter presenter,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetSetupStatusInputPort(), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpGet("settings")]
    [ProducesResponseType<GetHouseholdSettingsApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHouseholdSettings(
        [FromServices] GetHouseholdSettingsPresenter presenter,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetHouseholdSettingsInputPort(), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType<RegisterHouseholdApiResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterHousehold(
        [FromServices] RegisterHouseholdPresenter presenter,
        [FromBody] RegisterHouseholdApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new RegisterHouseholdInputPort(request.Email, request.FirstName, request.HouseholdName, request.LastName, request.Password), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPatch("settings")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateHouseholdSettings(
        [FromServices] UpdateHouseholdSettingsPresenter presenter,
        [FromBody] UpdateHouseholdSettingsApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new UpdateHouseholdSettingsInputPort(request.Latitude, request.LifxApiToken, request.Longitude, request.Name), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
