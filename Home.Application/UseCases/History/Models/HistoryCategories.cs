using Home.Domain.Enumerations;

namespace Home.Application.UseCases.History.Models;

/// <summary>
/// The one place a resource type becomes a category and back, so the feed's filter and the rows it
/// shows can never disagree about which is which.
/// </summary>
public static class HistoryCategories
{

    #region Fields

    private static readonly Dictionary<ResourceTypeSE, HistoryCategory> s_ByResource = new()
    {
        [ResourceTypeSE.Activity] = HistoryCategory.Chores,
        [ResourceTypeSE.MealPlanEntry] = HistoryCategory.Meals,
        [ResourceTypeSE.Recipe] = HistoryCategory.Recipes,
        [ResourceTypeSE.CalendarEvent] = HistoryCategory.Calendar,
        [ResourceTypeSE.CalendarSubscription] = HistoryCategory.Calendar,
        [ResourceTypeSE.ShoppingCart] = HistoryCategory.Shopping,
        [ResourceTypeSE.ShoppingCategory] = HistoryCategory.Shopping,
        [ResourceTypeSE.User] = HistoryCategory.Members,
    };

    #endregion Fields

    #region Methods

    /// <summary>
    /// The category a row belongs to, or null for a resource the feed does not show (notes).
    /// </summary>
    public static HistoryCategory? Of(ResourceTypeSE resourceType)
        => s_ByResource.TryGetValue(resourceType, out var _Category) ? _Category : null;

    /// <summary>
    /// The resource types a set of categories covers. Handed to the query as the enumeration
    /// instances themselves, because EF can translate a comparison against a value-converted column
    /// but not a member read off it.
    /// </summary>
    public static IReadOnlyList<ResourceTypeSE> ResourceTypesFor(IEnumerable<HistoryCategory> categories)
    {
        var _Wanted = categories.ToHashSet();

        return [.. s_ByResource.Where(kv => _Wanted.Contains(kv.Value)).Select(kv => kv.Key)];
    }

    #endregion Methods

}
