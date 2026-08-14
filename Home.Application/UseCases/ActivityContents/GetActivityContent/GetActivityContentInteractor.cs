using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ActivityContents.GetActivityContent;

internal class GetActivityContentInteractor : IInteractor<GetActivityContentInputPort, IGetActivityContentOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetActivityContentInputPort inputPort,
        IGetActivityContentOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _ActivityContent = _PersistenceContext.GetEntities<ActivityContent>()
            .Where(c => c.ActivityContentID == inputPort.ActivityContentID
                && c.Region.Activity.Household.HouseholdID == _Household.HouseholdID)
            .SingleOrDefault();

        if (_ActivityContent == null)
            await outputPort.PresentActivityContentNotFoundAsync(inputPort.ActivityContentID, cancellationToken);
        else
            await outputPort.PresentActivityContentAsync(_ActivityContent, cancellationToken);
    }

    #endregion Methods

}
