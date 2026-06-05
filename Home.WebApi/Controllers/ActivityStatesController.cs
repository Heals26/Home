using Home.Application.UseCases.ActivityStates.GetActivityStates;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.ActivityStates.GetActivityStates;
using Home.WebApi.UseCases.ActivityStates.GetActivityStates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class ActivityStatesController : BaseController
{

    #region Methods

    [HttpGet]
    [ProducesResponseType<GetActivityStatesApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivityStates(
        [FromServices] GetActivityStatesPresenter presenter,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetActivityStatesInputPort(), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
