using AutoMapper;
using Home.Application.UseCases.ShoppingLists.AddMealPlanToShoppingList;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.ShoppingLists.AddMealPlanToShoppingList;

namespace Home.WebApi.Presenters.ShoppingLists.AddMealPlanToShoppingList;

public class AddMealPlanToShoppingListPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IAddMealPlanToShoppingListOutputPort
{

    #region Methods

    Task IAddMealPlanToShoppingListOutputPort.PresentMealPlanAddedToShoppingListAsync(int recipeCount, CancellationToken cancellationToken)
        => this.OkAsync(new AddMealPlanToShoppingListApiResponse() { RecipeCount = recipeCount }, cancellationToken);

    Task IAddMealPlanToShoppingListOutputPort.PresentShoppingListNotFoundAsync(long shoppingListID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Shopping List {shoppingListID} Not Found", cancellationToken);

    #endregion Methods

}
