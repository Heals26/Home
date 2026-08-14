using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ActivityContents.GetActivityContents;

internal class GetActivityContentsInteractor : IInteractor<GetActivityContentsInputPort, IGetActivityContentsOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetActivityContentsInputPort inputPort,
        IGetActivityContentsOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Region = _PersistenceContext.GetEntities<ActivityRegion>()
            .Where(r => r.ActivityRegionID == inputPort.ActivityRegionID
                && r.Activity.Household.HouseholdID == _Household.HouseholdID)
            .Select(r => new
            {
                Region = r,
                r.Fields
            })
            .SingleOrDefault()
            ?.Region;

        if (_Region == null)
        {
            await outputPort.PresentActivityRegionNotFoundAsync(inputPort.ActivityRegionID, cancellationToken);
            return;
        }

        await outputPort.PresentActivityContentsAsync(_Region.Fields, cancellationToken);
    }

    #endregion Methods

}
