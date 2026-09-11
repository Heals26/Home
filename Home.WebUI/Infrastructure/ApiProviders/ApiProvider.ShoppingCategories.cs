using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetShoppingCategoriesBaseUrl()
        => $"{GetBaseApiUrl()}/ShoppingCategories";

    private static string GetShoppingCategoryBaseUrl(long shoppingCategoryID)
        => $"{GetShoppingCategoriesBaseUrl()}/{shoppingCategoryID}";

    #endregion Base

    #region Methods

    public static ApiProviderHelper CreateShoppingCategory()
        => new(HttpMethod.Post, RouteType.Body, GetShoppingCategoriesBaseUrl());

    public static ApiProviderHelper DeleteShoppingCategory(long shoppingCategoryID)
        => new(HttpMethod.Delete, RouteType.Route, GetShoppingCategoryBaseUrl(shoppingCategoryID));

    public static ApiProviderHelper GetShoppingCategories()
        => new(HttpMethod.Get, RouteType.Route, GetShoppingCategoriesBaseUrl());

    public static ApiProviderHelper UpdateShoppingCategory(long shoppingCategoryID)
        => new(HttpMethod.Patch, RouteType.Body, GetShoppingCategoryBaseUrl(shoppingCategoryID));

    #endregion Methods

}
