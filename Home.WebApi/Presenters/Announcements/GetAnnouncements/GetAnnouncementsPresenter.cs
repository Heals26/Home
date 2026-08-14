using AutoMapper;
using Home.Application.UseCases.Announcements.GetAnnouncements;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Announcements.GetAnnouncements;
using Home.WebApi.UseCases.Announcements.Models;

namespace Home.WebApi.Presenters.Announcements.GetAnnouncements;

public class GetAnnouncementsPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetAnnouncementsOutputPort
{

    #region Methods

    Task IGetAnnouncementsOutputPort.PresentAnnouncementsAsync(IEnumerable<Announcement> announcements, CancellationToken cancellationToken)
        => this.OkAsync(new GetAnnouncementsApiResponse()
        {
            Announcements = [.. announcements.Select(a => new AnnouncementDto()
            {
                AnnouncementID = a.AnnouncementID,
                Content = a.Content,
                CreatedOnUTC = a.CreatedOnUTC
            })]
        }, cancellationToken);

    #endregion Methods

}
