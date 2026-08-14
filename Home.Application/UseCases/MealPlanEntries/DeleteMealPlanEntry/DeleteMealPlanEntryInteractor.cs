using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.MealPlanEntries.DeleteMealPlanEntry;

internal class DeleteMealPlanEntryInteractor
    : IInteractor<DeleteMealPlanEntryInputPort, IDeleteMealPlanEntryOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        DeleteMealPlanEntryInputPort inputPort,
        IDeleteMealPlanEntryOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Entry = _PersistenceContext.GetEntities<MealPlanEntry>()
            .Where(e => e.MealPlanEntryID == inputPort.MealPlanEntryID
                && e.Recipe.Household.HouseholdID == _Household.HouseholdID)
            .SingleOrDefault();

        if (_Entry == null)
        {
            await outputPort.PresentMealPlanEntryNotFoundAsync(inputPort.MealPlanEntryID, cancellationToken);
            return;
        }

        _PersistenceContext.Remove(_Entry);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentMealPlanEntryDeletedAsync(cancellationToken);
    }

    #endregion Methods

}
