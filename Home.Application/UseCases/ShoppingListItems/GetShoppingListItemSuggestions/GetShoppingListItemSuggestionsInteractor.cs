using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingListItems.GetShoppingListItemSuggestions;

internal class GetShoppingListItemSuggestionsInteractor : IInteractor<GetShoppingListItemSuggestionsInputPort, IGetShoppingListItemSuggestionsOutputPort>
{

    #region Fields

    /// <summary>
    /// The whole set goes to the caller at once so typing filters it there rather than asking the
    /// server on every keystroke. Small enough to be cheap on a phone, long enough to cover a
    /// household's real vocabulary.
    /// </summary>
    private const int SuggestionLimit = 200;

    #endregion Fields

    #region Methods

    public async Task HandleAsync(
        GetShoppingListItemSuggestionsInputPort inputPort,
        IGetShoppingListItemSuggestionsOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Grouped = _PersistenceContext.GetEntities<ShoppingListItem>()
            .Where(sli => sli.ShoppingList.Household.HouseholdID == _Household.HouseholdID)
            .GroupBy(sli => sli.Name)
            .Select(g => new
            {
                Name = g.Key,
                TimesAdded = g.LongCount(),
                LastAddedID = g.Max(sli => sli.ShoppingListItemID)
            })
            .OrderByDescending(s => s.TimesAdded)
            .ThenBy(s => s.Name)
            .Take(SuggestionLimit)
            .ToList();

        var _LastAddedIDs = _Grouped.Select(s => s.LastAddedID).ToList();

        // The amount and price come from the most recent time it was added rather than an average,
        // because the last shop is the best guess at the next one.
        var _LastAdded = _PersistenceContext.GetEntities<ShoppingListItem>()
            .Where(sli => _LastAddedIDs.Contains(sli.ShoppingListItemID))
            .Select(sli => new
            {
                sli.ShoppingListItemID,
                sli.Amount,
                sli.Cost,
                sli.Unit
            })
            .ToDictionary(sli => sli.ShoppingListItemID);

        var _Suggestions = _Grouped
            .Select(s => new { Suggestion = s, Last = _LastAdded.GetValueOrDefault(s.LastAddedID) })
            .Select(s => new ShoppingListItemSuggestion(s.Last?.Amount, s.Last?.Cost, s.Suggestion.Name, s.Suggestion.TimesAdded, s.Last?.Unit));

        await outputPort.PresentShoppingListItemSuggestionsAsync(_Suggestions, cancellationToken).ConfigureAwait(false);
    }

    #endregion Methods

}
