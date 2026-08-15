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
    public bool InBasket { get; set; }
    public long ShoppingListItemID { get; set; }
    public string Name { get; set; }

    /// <summary>
    /// Kept only so rows written before amounts carried a unit still read correctly.
    /// </summary>
    public decimal? Quantity { get; set; }

    public long Sequence { get; set; }
    public long? Unit { get; set; }

    public string UnitAbbreviation
        => MeasurementUnitLogic.GetAbbreviation(this.Unit);

    public decimal? Volume { get; set; }
    public decimal? Weight { get; set; }

    #endregion Properties

}
