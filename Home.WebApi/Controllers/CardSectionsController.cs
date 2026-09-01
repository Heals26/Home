using Home.Application.UseCases.CardSections.CreateCardSection;
using Home.Application.UseCases.CardSections.DeleteCardSection;
using Home.Application.UseCases.CardSections.GetCardSections;
using Home.Application.UseCases.CardSections.UpdateCardSection;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.CardSections.CreateCardSection;
using Home.WebApi.Presenters.CardSections.DeleteCardSection;
using Home.WebApi.Presenters.CardSections.GetCardSections;
using Home.WebApi.Presenters.CardSections.UpdateCardSection;
using Home.WebApi.UseCases.CardSections.CreateCardSection;
using Home.WebApi.UseCases.CardSections.GetCardSections;
using Home.WebApi.UseCases.CardSections.UpdateCardSection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class CardSectionsController : BaseController
{

    #region Methods

    [HttpPost]
    [ProducesResponseType<CreateCardSectionApiResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateCardSection(
        [FromServices] CreateCardSectionPresenter presenter,
        [FromBody] CreateCardSectionApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new CreateCardSectionInputPort(request.Name), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpDelete("{cardSectionID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCardSection(
        [FromServices] DeleteCardSectionPresenter presenter,
        [FromRoute] long cardSectionID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new DeleteCardSectionInputPort(cardSectionID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpGet]
    [ProducesResponseType<GetCardSectionsApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCardSections(
        [FromServices] GetCardSectionsPresenter presenter,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetCardSectionsInputPort(), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPatch("{cardSectionID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateCardSection(
        [FromServices] UpdateCardSectionPresenter presenter,
        [FromRoute] long cardSectionID,
        [FromBody] UpdateCardSectionApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new UpdateCardSectionInputPort(cardSectionID, request.Name, request.Sequence), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}