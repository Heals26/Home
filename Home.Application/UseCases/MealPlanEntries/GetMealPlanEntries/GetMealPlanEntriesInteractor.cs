using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.MealPlanEntries.GetMealPlanEntries;

internal class GetMealPlanEntriesInteractor
    : IInteractor<GetMealPlanEntriesInputPort, IGetMealPlanEntriesOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetMealPlanEntriesInputPort inputPort,
        IGetMealPlanEntriesOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        // Dates compare on the day only — entries always store midnight.
        var _FromDate = inputPort.FromDate.Date;
        var _ToDate = inputPort.ToDate.Date;

        var _Entries = _PersistenceContext.GetEntities<MealPlanEntry>()
            .Where(e => e.Recipe.Household.HouseholdID == _Household.HouseholdID
                && e.Date >= _FromDate
                && e.Date <= _ToDate)
            .Select(e => new { Entry = e, e.Recipe })
            .ToList()
            .Select(e => e.Entry)
            .OrderBy(e => e.Date)
            .ThenBy(e => e.MealPlanEntryID)
            .ToList();

        await outputPort.PresentMealPlanEntriesAsync(_Entries, cancellationToken);
    }

    #endregion Methods

}
