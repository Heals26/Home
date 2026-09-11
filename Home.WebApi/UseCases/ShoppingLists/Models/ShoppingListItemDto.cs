using Home.Application.Infrastructure.Recipes;

namespace Home.WebApi.UseCases.ShoppingLists.Models;

public class ShoppingListItemDto
{

    #region Properties

    /// <summary>
    /// How much to buy, in <see cref="Unit"/>.
    /// </summary>
    public decimal? Amount { get; set; }

    public decimal? Cost { get; set; }

    /// <summary>
    /// For a line with no price, roughly what it will cost, from what the household paid last time.
    /// </summary>
    public decimal? EstimatedCost { get; set; }

    public bool InBasket { get; set; }
    public bool IsDearerThanUsual { get; set; }
    public long ShoppingListItemID { get; set; }
    public string Name { get; set; }

    /// <summary>
    /// What the name does not say: a brand, a size, which aisle, who it is for. Null when the line
    /// carries nothing extra.
    /// </summary>
    public string Note { get; set; }

    public long Sequence { get; set; }

    /// <summary>
    /// The aisle, filed against the item's name, so it is the same on every list.
    /// </summary>
    public long? ShoppingCategoryID { get; set; }

    public long? Unit { get; set; }

    public string UnitAbbreviation
        => MeasurementUnitLogic.GetAbbreviation(this.Unit, this.Amount);

    /// <summary>
    /// What this line usually costs at this amount, worked out per unit from past purchases.
    /// </summary>
    public decimal? UsualCost { get; set; }

    #endregion Properties

}
