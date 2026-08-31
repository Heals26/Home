using Home.Application.Infrastructure.Recipes;

namespace Home.WebApi.UseCases.ShoppingListItems.GetShoppingListItem;

/// <summary>
/// Gets the item in the shopping list. Quantity, Volume and Weight are kept only so rows
/// written before amounts carried a unit still read correctly.
/// </summary>
public record GetShoppingListItemApiResponse(
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
        => MeasurementUnitLogic.GetAbbreviation(this.Unit, this.Amount);

    #endregion Properties

}
