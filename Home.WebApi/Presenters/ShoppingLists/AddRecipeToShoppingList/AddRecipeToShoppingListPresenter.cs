using AutoMapper;
using Home.Application.UseCases.ShoppingLists.AddRecipeToShoppingList;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.ShoppingLists.AddRecipeToShoppingList;

public class AddRecipeToShoppingListPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IAddRecipeToShoppingListOutputPort
{

    #region Methods

    Task IAddRecipeToShoppingListOutputPort.PresentRecipeAddedToShoppingListAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IAddRecipeToShoppingListOutputPort.PresentRecipeNotFoundAsync(long recipeID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Recipe {recipeID} Not Found", cancellationToken);

    Task IAddRecipeToShoppingListOutputPort.PresentShoppingListNotFoundAsync(long shoppingListID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Shopping List {shoppingListID} Not Found", cancellationToken);

    #endregion Methods

}
