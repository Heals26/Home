using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;

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
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<MealPlanEntry>>();

        var _Household = _AuthorisationService.GetHousehold();

        // Recipe and slot are projected because the history names them.
        var _Entry = _PersistenceContext.GetEntities<MealPlanEntry>()
            .Where(e => e.MealPlanEntryID == inputPort.MealPlanEntryID
                && e.Household.HouseholdID == _Household.HouseholdID)
            .Select(e => new
            {
                Entry = e,
                e.MealSlot,
                e.Recipe
            })
            .SingleOrDefault()
            ?.Entry;

        if (_Entry == null)
        {
            await outputPort.PresentMealPlanEntryNotFoundAsync(inputPort.MealPlanEntryID, cancellationToken);
            return;
        }

        _AuditLogic.DeleteAudit(_Entry);
        _PersistenceContext.Remove(_Entry);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentMealPlanEntryDeletedAsync(cancellationToken);
    }

    #endregion Methods

}
