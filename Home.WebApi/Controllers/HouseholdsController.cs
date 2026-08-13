using Home.Application.UseCases.Households.GetHouseholdSettings;
using Home.Application.UseCases.Households.UpdateHouseholdSettings;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.Households.GetHouseholdSettings;
using Home.WebApi.Presenters.Households.UpdateHouseholdSettings;
using Home.WebApi.UseCases.Households.GetHouseholdSettings;
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

    [HttpGet("settings")]
    [ProducesResponseType<GetHouseholdSettingsApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHouseholdSettings(
        [FromServices] GetHouseholdSettingsPresenter presenter,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetHouseholdSettingsInputPort(), presenter, this.ServiceFactory, cancellationToken);

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
