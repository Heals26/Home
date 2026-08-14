using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ActivityContents.DeleteActivityContent;

internal class DeleteActivityContentInteractor : IInteractor<DeleteActivityContentInputPort, IDeleteActivityContentOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        DeleteActivityContentInputPort inputPort,
        IDeleteActivityContentOutputPort outputPort,
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
        {
            await outputPort.PresentActivityContentNotFoundAsync(inputPort.ActivityContentID, cancellationToken);
            return;
        }

        _PersistenceContext.Remove(_ActivityContent);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentActivityContentDeletedNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
