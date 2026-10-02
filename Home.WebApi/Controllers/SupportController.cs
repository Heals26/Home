using Home.Application.UseCases.Support.CreateFeedback;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.Support.CreateFeedback;
using Home.WebApi.UseCases.Support.CreateFeedback;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class SupportController : BaseController
{

    #region Methods

    [HttpPost("Feedback")]
    [ProducesResponseType<CreateFeedbackApiResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateFeedback(
        [FromServices] CreateFeedbackPresenter presenter,
        [FromBody] CreateFeedbackApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new CreateFeedbackInputPort(request.Content, request.Kind, request.Page, request.ScreenHeight, request.ScreenWidth), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
