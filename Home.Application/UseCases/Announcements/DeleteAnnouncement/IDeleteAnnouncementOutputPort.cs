namespace Home.Application.UseCases.Announcements.DeleteAnnouncement;

public interface IDeleteAnnouncementOutputPort
{

    #region Methods

    Task PresentAnnouncementDeletedAsync(CancellationToken cancellationToken);
    Task PresentAnnouncementNotFoundAsync(long announcementID, CancellationToken cancellationToken);

    #endregion Methods

}
