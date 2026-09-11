using Home.WebUI.DataAccess.ShoppingCategories.Models;
using Home.WebUI.DataAccess.ShoppingLists.Models;

namespace Home.WebUI.Infrastructure.Services.ShoppingLists;

/// <summary>
/// One aisle's share of a list, in the list's own order.
/// </summary>
/// <param name="Aisle">Null for the items not filed under any aisle yet.</param>
public record ShoppingAisleGroup(ShoppingCategoryDto? Aisle, IReadOnlyList<ShoppingListItemDto> Items);
