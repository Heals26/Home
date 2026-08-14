using Home.Application.UseCases.Announcements.CreateAnnouncement;
using Home.Application.UseCases.Announcements.DeleteAnnouncement;
using Home.Application.UseCases.Announcements.GetAnnouncements;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.Announcements.CreateAnnouncement;
using Home.WebApi.Presenters.Announcements.DeleteAnnouncement;
using Home.WebApi.Presenters.Announcements.GetAnnouncements;
using Home.WebApi.UseCases.Announcements.CreateAnnouncement;
using Home.WebApi.UseCases.Announcements.GetAnnouncements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class AnnouncementsController : BaseController
{

    #region Methods

    [HttpPost]
    [ProducesResponseType<CreateAnnouncementApiResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAnnouncement(
        [FromServices] CreateAnnouncementPresenter presenter,
        [FromBody] CreateAnnouncementApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new CreateAnnouncementInputPort(request.Content), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpDelete("{announcementID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAnnouncement(
        [FromServices] DeleteAnnouncementPresenter presenter,
        [FromRoute] long announcementID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new DeleteAnnouncementInputPort(announcementID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpGet]
    [ProducesResponseType<GetAnnouncementsApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnnouncements(
        [FromServices] GetAnnouncementsPresenter presenter,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetAnnouncementsInputPort(), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
