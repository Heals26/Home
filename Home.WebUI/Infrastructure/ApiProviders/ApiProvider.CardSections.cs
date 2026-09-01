using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetCardSectionBaseUrl(long cardSectionID)
        => $"{GetCardSectionsBaseUrl()}/{cardSectionID}";

    private static string GetCardSectionsBaseUrl()
        => $"{GetBaseApiUrl()}/CardSections";

    #endregion Base

    #region Methods

    public static ApiProviderHelper CreateCardSection()
        => new(HttpMethod.Post, RouteType.Body, GetCardSectionsBaseUrl());

    public static ApiProviderHelper DeleteCardSection(long cardSectionID)
        => new(HttpMethod.Delete, RouteType.Route, GetCardSectionBaseUrl(cardSectionID));

    public static ApiProviderHelper GetCardSections()
        => new(HttpMethod.Get, RouteType.Route, GetCardSectionsBaseUrl());

    public static ApiProviderHelper UpdateCardSection(long cardSectionID)
        => new(HttpMethod.Patch, RouteType.Body, GetCardSectionBaseUrl(cardSectionID));

    #endregion Methods

}