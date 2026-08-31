using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.RecipeIngredients.GetIngredientSuggestions;

internal class GetIngredientSuggestionsInteractor : IInteractor<GetIngredientSuggestionsInputPort, IGetIngredientSuggestionsOutputPort>
{

    #region Fields

    /// <summary>
    /// The whole set goes to the caller at once so typing filters it there rather than asking the
    /// server on every keystroke, the same way the shopping list's suggestions work.
    /// </summary>
    private const int SuggestionLimit = 200;

    #endregion Fields

    #region Methods

    public async Task HandleAsync(
        GetIngredientSuggestionsInputPort inputPort,
        IGetIngredientSuggestionsOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        // An ingredient carries no household of its own — it is reached through the recipes it
        // belongs to, which is what keeps one household's larder out of another's.
        var _Grouped = _PersistenceContext.GetEntities<Ingredient>()
            .Where(i => i.Recipes.Any(ri => ri.Recipe.Household.HouseholdID == _Household.HouseholdID))
            .GroupBy(i => i.Name)
            .Select(g => new
            {
                Name = g.Key,
                TimesUsed = g.LongCount(),
                LastUsedID = g.Max(i => i.IngredientID)
            })
            .OrderByDescending(s => s.TimesUsed)
            .ThenBy(s => s.Name)
            .Take(SuggestionLimit)
            .ToList();

        var _LastUsedIDs = _Grouped.Select(s => s.LastUsedID).ToList();

        // The amount comes from the most recent time it was written rather than an average, because
        // the last recipe to use it is the best guess at the next one.
        var _LastUsed = _PersistenceContext.GetEntities<Ingredient>()
            .Where(i => _LastUsedIDs.Contains(i.IngredientID))
            .Select(i => new
            {
                i.IngredientID,
                i.Amount,
                i.Unit
            })
            .ToDictionary(i => i.IngredientID);

        var _Suggestions = _Grouped
            .Select(s => new { Suggestion = s, Last = _LastUsed.GetValueOrDefault(s.LastUsedID) })
            .Select(s => new IngredientSuggestion(s.Last?.Amount, s.Suggestion.Name, s.Suggestion.TimesUsed, s.Last?.Unit));

        await outputPort.PresentIngredientSuggestionsAsync(_Suggestions, cancellationToken).ConfigureAwait(false);
    }

    #endregion Methods

}
