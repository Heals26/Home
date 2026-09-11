namespace Home.WebUI.Infrastructure.Services.ShoppingLists;

/// <summary>
/// What a list comes to before anyone leaves the house.
/// </summary>
/// <param name="IncludesGuesses">Part of the total is what an unpriced line cost last time.</param>
/// <param name="Total">The prices on the list, plus the guesses.</param>
/// <param name="Unpriced">Lines with no price and nothing to go on, which the total leaves out.</param>
public record ShoppingListEstimate(bool IncludesGuesses, decimal Total, int Unpriced);
