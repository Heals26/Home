using Home.Application.UseCases.Tags.CreateTag;
using Home.Application.UseCases.Tags.DeleteTag;
using Home.Application.UseCases.Tags.GetTags;
using Home.Application.UseCases.Tags.UpdateTag;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.Tags.CreateTag;
using Home.WebApi.Presenters.Tags.DeleteTag;
using Home.WebApi.Presenters.Tags.GetTags;
using Home.WebApi.Presenters.Tags.UpdateTag;
using Home.WebApi.UseCases.Tags.CreateTag;
using Home.WebApi.UseCases.Tags.GetTags;
using Home.WebApi.UseCases.Tags.UpdateTag;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class TagsController : BaseController
{

    #region Methods

    [HttpPost]
    [ProducesResponseType<CreateTagApiResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateTag(
        [FromServices] CreateTagPresenter presenter,
        [FromBody] CreateTagApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new CreateTagInputPort(request.Colour, request.Name), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpDelete("{tagID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTag(
        [FromServices] DeleteTagPresenter presenter,
        [FromRoute] long tagID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new DeleteTagInputPort(tagID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpGet]
    [ProducesResponseType<GetTagsApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTags(
        [FromServices] GetTagsPresenter presenter,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetTagsInputPort(), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPatch("{tagID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateTag(
        [FromServices] UpdateTagPresenter presenter,
        [FromRoute] long tagID,
        [FromBody] UpdateTagApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new UpdateTagInputPort(tagID, request.Colour, request.Name), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
