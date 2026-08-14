using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Announcements.DeleteAnnouncement;

internal class DeleteAnnouncementInteractor : IInteractor<DeleteAnnouncementInputPort, IDeleteAnnouncementOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        DeleteAnnouncementInputPort inputPort,
        IDeleteAnnouncementOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Announcement = _PersistenceContext.GetEntities<Announcement>()
            .Where(a => a.AnnouncementID == inputPort.AnnouncementID
                && a.Household.HouseholdID == _Household.HouseholdID)
            .SingleOrDefault();

        if (_Announcement == null)
        {
            await outputPort.PresentAnnouncementNotFoundAsync(inputPort.AnnouncementID, cancellationToken);
            return;
        }

        _PersistenceContext.Remove(_Announcement);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentAnnouncementDeletedAsync(cancellationToken);
    }

    #endregion Methods

}
