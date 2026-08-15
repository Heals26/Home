using AutoMapper;
using Home.Application.UseCases.Recipes.SetRecipeMealSlots;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.Recipes.SetRecipeMealSlots;

public class SetRecipeMealSlotsPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ISetRecipeMealSlotsOutputPort
{

    #region Methods

    Task ISetRecipeMealSlotsOutputPort.PresentMealSlotNotFoundAsync(long mealSlotID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Meal Slot {mealSlotID} Not Found", cancellationToken);

    Task ISetRecipeMealSlotsOutputPort.PresentRecipeMealSlotsSetAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task ISetRecipeMealSlotsOutputPort.PresentRecipeNotFoundAsync(long recipeID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Recipe {recipeID} Not Found", cancellationToken);

    #endregion Methods

}
