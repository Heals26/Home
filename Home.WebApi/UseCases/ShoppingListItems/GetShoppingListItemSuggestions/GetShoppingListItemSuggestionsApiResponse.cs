using Home.Application.Infrastructure.Recipes;

namespace Home.WebApi.UseCases.ShoppingListItems.GetShoppingListItemSuggestions;

public class GetShoppingListItemSuggestionsApiResponse
{

    #region Properties

    /// <summary>
    /// Things the household has bought before, most often bought first
    /// </summary>
    public ICollection<GetShoppingListItemSuggestionDto> Suggestions { get; set; }

    #endregion Properties

}

public class GetShoppingListItemSuggestionDto
{

    #region Properties

    /// <summary>
    /// The amount it was last added with
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// What it last cost
    /// </summary>
    public decimal? Cost { get; set; }

    /// <summary>
    /// The item as it was last written
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// How often the household has added it
    /// </summary>
    public long TimesAdded { get; set; }

    /// <summary>
    /// The measurement the amount was last in
    /// </summary>
    public long? Unit { get; set; }

    /// <summary>
    /// How the unit reads beside the amount
    /// </summary>
    public string UnitAbbreviation
        => MeasurementUnitLogic.GetAbbreviation(this.Unit, this.Amount);

    #endregion Properties

}
