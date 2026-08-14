using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.Activities;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ActivityContents.UpdateActivityContent;

internal class UpdateActivityContentInteractor : IInteractor<UpdateActivityContentInputPort, IUpdateActivityContentOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        UpdateActivityContentInputPort inputPort,
        IUpdateActivityContentOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _ActivityLogic = serviceFactory.GetService<IActivityLogic>();

        var _Household = _AuthorisationService.GetHousehold();

        var _ActivityContentExists = _PersistenceContext.GetEntities<ActivityContent>()
            .Any(c => c.ActivityContentID == inputPort.ActivityContentID
                && c.Region.Activity.Household.HouseholdID == _Household.HouseholdID);

        if (!_ActivityContentExists)
        {
            await outputPort.PresentActivityContentNotFoundAsync(inputPort.ActivityContentID, cancellationToken);
            return;
        }

        _ActivityLogic.UpdateContent(inputPort);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentActivityContentNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
