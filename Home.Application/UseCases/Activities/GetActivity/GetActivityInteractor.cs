using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Activities.GetActivity;

internal class GetActivityInteractor : IInteractor<GetActivityInputPort, IGetActivityOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetActivityInputPort inputPort,
        IGetActivityOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Activity = _PersistenceContext.GetEntities<Activity>()
            .Where(a => a.ActivityID == inputPort.ActivityID && a.Household.HouseholdID == _Household.HouseholdID)
            .Select(a => new
            {
                Activity = a,
                // CardSection is projected because the presenter reads its name off every region.
                // Leaving it out loads the region with a null section and the card fails to open.
                Regions = a.Regions.Select(r => new
                {
                    Region = r,
                    r.CardSection,
                    r.Fields
                }),
                a.State,
                Tags = a.Tags.Select(t => new { ActivityTag = t, t.Tag }),
                a.User
            })
            .SingleOrDefault()
            ?.Activity;

        if (_Activity == null)
            await outputPort.PresentActivityNotFoundAsync(inputPort.ActivityID, cancellationToken);
        else
            await outputPort.PresentActivityAsync(_Activity, cancellationToken);
    }

    #endregion Methods

}
