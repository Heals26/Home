using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.Announcements.CreateAnnouncement;

public interface ICreateAnnouncementOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task PresentAnnouncementCreatedAsync(long announcementID, CancellationToken cancellationToken);

    #endregion Methods

}
