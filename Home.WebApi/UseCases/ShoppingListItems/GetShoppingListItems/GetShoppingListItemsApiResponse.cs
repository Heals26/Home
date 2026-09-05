using Home.Application.Infrastructure.Recipes;

namespace Home.WebApi.UseCases.ShoppingListItems.GetShoppingListItems;

/// <summary>
/// Everything on one shopping list, in the order it was written.
/// </summary>
public record GetShoppingListItemsApiResponse(List<GetShoppingListItemDto> Items);

/// <summary>
/// One line on a shopping list.
/// </summary>
public record GetShoppingListItemDto(
    decimal? Amount,
    decimal? Cost,
    bool InBasket,
    string Name,
    long Sequence,
    long ShoppingListItemID,
    long? Unit)
{

    #region Properties

    public string UnitAbbreviation
        => MeasurementUnitLogic.GetAbbreviation(this.Unit, this.Amount);

    #endregion Properties

}
