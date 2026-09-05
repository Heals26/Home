using Home.Application.Infrastructure.Recipes;

namespace Home.WebApi.UseCases.ShoppingListItems.GetShoppingListItem;

/// <summary>
/// One line on a shopping list.
/// </summary>
public record GetShoppingListItemApiResponse(
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
