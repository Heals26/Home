using AutoMapper;
using Home.Application.UseCases.Announcements.DeleteAnnouncement;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.Announcements.DeleteAnnouncement;

public class DeleteAnnouncementPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IDeleteAnnouncementOutputPort
{

    #region Methods

    Task IDeleteAnnouncementOutputPort.PresentAnnouncementDeletedAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IDeleteAnnouncementOutputPort.PresentAnnouncementNotFoundAsync(long announcementID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Announcement {announcementID} Not Found", cancellationToken);

    #endregion Methods

}
