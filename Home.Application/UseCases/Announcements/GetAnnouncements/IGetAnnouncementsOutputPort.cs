using Home.Domain.Entities;

namespace Home.Application.UseCases.Announcements.GetAnnouncements;

public interface IGetAnnouncementsOutputPort
{

    #region Methods

    Task PresentAnnouncementsAsync(IEnumerable<Announcement> announcements, CancellationToken cancellationToken);

    #endregion Methods

}
