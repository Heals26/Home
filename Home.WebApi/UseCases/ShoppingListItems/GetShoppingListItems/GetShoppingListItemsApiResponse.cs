using Home.Application.Infrastructure.Recipes;

namespace Home.WebApi.UseCases.ShoppingListItems.GetShoppingListItems;

/// <summary>
/// Gets the shopping list items
/// </summary>
public record GetShoppingListItemsApiResponse(List<GetShoppingListItemDto> Items);

/// <summary>
/// A shopping list item. Quantity, Volume and Weight are kept only so rows written before
/// amounts carried a unit still read correctly.
/// </summary>
public record GetShoppingListItemDto(
    decimal? Amount,
    decimal? Cost,
    bool InBasket,
    string Name,
    decimal? Quantity,
    long Sequence,
    long ShoppingListItemID,
    long? Unit,
    decimal? Volume,
    decimal? Weight)
{

    #region Properties

    public string UnitAbbreviation
        => MeasurementUnitLogic.GetAbbreviation(this.Unit);

    #endregion Properties

}
