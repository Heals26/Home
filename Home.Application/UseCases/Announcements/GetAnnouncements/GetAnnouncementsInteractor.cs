using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Announcements.GetAnnouncements;

internal class GetAnnouncementsInteractor : IInteractor<GetAnnouncementsInputPort, IGetAnnouncementsOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetAnnouncementsInputPort inputPort,
        IGetAnnouncementsOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Announcements = _PersistenceContext.GetEntities<Announcement>()
            .Where(a => a.Household.HouseholdID == _Household.HouseholdID)
            .OrderByDescending(a => a.CreatedOnUTC)
            .ToList();

        await outputPort.PresentAnnouncementsAsync(_Announcements, cancellationToken);
    }

    #endregion Methods

}
