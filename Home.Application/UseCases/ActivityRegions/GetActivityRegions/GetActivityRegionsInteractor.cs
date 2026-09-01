using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ActivityRegions.GetActivityRegions;

internal class GetActivityRegionsInteractor : IInteractor<GetActivityRegionsInputPort, IGetActivityRegionsOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetActivityRegionsInputPort inputPort,
        IGetActivityRegionsOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Activity = _PersistenceContext.GetEntities<Activity>()
            .Where(a => a.ActivityID == inputPort.ActivityID
                && a.Household.HouseholdID == _Household.HouseholdID)
            .Select(a => new
            {
                Activity = a,
                Regions = a.Regions.Select(r => new
                {
                    Region = r,
                    r.CardSection,
                    r.Fields
                })
            })
            .SingleOrDefault()
            ?.Activity;

        if (_Activity == null)
        {
            await outputPort.PresentActivityNotFoundAsync(inputPort.ActivityID, cancellationToken);
            return;
        }

        await outputPort.PresentActivityRegionsAsync(_Activity.Regions, cancellationToken);
    }

    #endregion Methods

}
