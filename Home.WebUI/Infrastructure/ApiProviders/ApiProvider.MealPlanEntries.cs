using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetMealPlanEntriesBaseUrl()
        => $"{GetBaseApiUrl()}/MealPlanEntries";

    private static string GetMealPlanEntryBaseUrl(long mealPlanEntryID)
        => $"{GetMealPlanEntriesBaseUrl()}/{mealPlanEntryID}";

    #endregion Base

    #region Methods

    public static ApiProviderHelper CreateMealPlanEntry()
        => new(HttpMethod.Post, RouteType.Body, GetMealPlanEntriesBaseUrl());

    public static ApiProviderHelper DeleteMealPlanEntry(long mealPlanEntryID)
        => new(HttpMethod.Delete, RouteType.Route, GetMealPlanEntryBaseUrl(mealPlanEntryID));

    public static ApiProviderHelper GetMealPlanEntries(DateTime fromDate, DateTime toDate)
        => new(HttpMethod.Get, RouteType.Route, $"{GetMealPlanEntriesBaseUrl()}?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}");

    #endregion Methods

}
