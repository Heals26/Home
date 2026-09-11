namespace Home.Application.UseCases.ShoppingLists.Models;

/// <summary>
/// What the household's memory says about one line on a list.
/// </summary>
/// <param name="EstimatedCost">For a line with no price, what it will probably cost. Null when the
/// line has a price or the item was never bought with one.</param>
/// <param name="IsDearerThanUsual">The line's own price is more than the margin over usual.</param>
/// <param name="ShoppingCategoryID">The aisle the item is filed under, or null.</param>
/// <param name="UsualCost">Usual, worked out for this line's amount so it reads beside the line's own
/// price. Null when nothing comparable was bought before.</param>
public record ShoppingItemInsight(decimal? EstimatedCost, bool IsDearerThanUsual, long? ShoppingCategoryID, decimal? UsualCost);
