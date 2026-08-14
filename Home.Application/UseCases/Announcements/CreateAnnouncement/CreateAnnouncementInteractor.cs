using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Announcements.CreateAnnouncement;

internal class CreateAnnouncementInteractor : IInteractor<CreateAnnouncementInputPort, ICreateAnnouncementOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        CreateAnnouncementInputPort inputPort,
        ICreateAnnouncementOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _TimeProvider = serviceFactory.GetService<TimeProvider>();

        var _Announcement = new Announcement()
        {
            Content = inputPort.Content.Trim(),
            CreatedOnUTC = _TimeProvider.GetUtcNow().UtcDateTime,
            Household = _AuthorisationService.GetHousehold()
        };

        _PersistenceContext.Add(_Announcement);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentAnnouncementCreatedAsync(_Announcement.AnnouncementID, cancellationToken);
    }

    #endregion Methods

}
