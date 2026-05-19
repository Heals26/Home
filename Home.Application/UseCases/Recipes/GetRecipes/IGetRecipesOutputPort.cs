using Home.Domain.Entities;

namespace Home.Application.UseCases.Recipes.GetRecipes;

public interface IGetRecipesOutputPort
{

    #region Methods

    Task PresentRecipesAsync(IEnumerable<Recipe> recipes, CancellationToken cancellationToken);

    #endregion Methods

}
