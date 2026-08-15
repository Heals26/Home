using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetMealSlotBaseUrl(long mealSlotID)
        => $"{GetMealSlotsBaseUrl()}/{mealSlotID}";

    private static string GetMealSlotsBaseUrl()
        => $"{GetBaseApiUrl()}/MealSlots";

    #endregion Base

    #region Methods

    public static ApiProviderHelper CreateMealSlot()
        => new(HttpMethod.Post, RouteType.Body, GetMealSlotsBaseUrl());

    public static ApiProviderHelper DeleteMealSlot(long mealSlotID)
        => new(HttpMethod.Delete, RouteType.Route, GetMealSlotBaseUrl(mealSlotID));

    public static ApiProviderHelper GetMealSlots()
        => new(HttpMethod.Get, RouteType.Route, GetMealSlotsBaseUrl());

    public static ApiProviderHelper UpdateMealSlot(long mealSlotID)
        => new(HttpMethod.Patch, RouteType.Body, GetMealSlotBaseUrl(mealSlotID));

    #endregion Methods

}
