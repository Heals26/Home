using Home.Application.UseCases.Recipes.Models;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Recipes.GetRecipes;

public interface IGetRecipesOutputPort
{

    #region Methods

    /// <summary>
    /// The recipes, and what the planner remembers about each by recipe ID. A recipe with no entry
    /// in the dictionary has never been planned.
    /// </summary>
    Task PresentRecipesAsync(IEnumerable<Recipe> recipes, IReadOnlyDictionary<long, RecipeMealHistory> history, CancellationToken cancellationToken);

    #endregion Methods

}
