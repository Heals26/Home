using Home.Domain.Entities;

namespace Home.Application.Services.EntityLogic.Recipes;

public interface IRecipeLogic
{

    #region Methods

    void AddIngredientsToShoppingList(Recipe recipe, ShoppingList shoppingList);

    #endregion Methods

}
