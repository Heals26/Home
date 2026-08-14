using AutoMapper;
using Home.Application.UseCases.Announcements.CreateAnnouncement;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Announcements.CreateAnnouncement;

namespace Home.WebApi.Presenters.Announcements.CreateAnnouncement;

public class CreateAnnouncementPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ICreateAnnouncementOutputPort
{

    #region Methods

    Task ICreateAnnouncementOutputPort.PresentAnnouncementCreatedAsync(long announcementID, CancellationToken cancellationToken)
        => this.CreatedAsync(announcementID, new CreateAnnouncementApiResponse() { AnnouncementID = announcementID }, cancellationToken);

    #endregion Methods

}
