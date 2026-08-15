using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetActivityStateBaseUrl(long activityStateID)
        => $"{GetActivityStatesBaseUrl()}/{activityStateID}";

    private static string GetActivityStatesBaseUrl()
        => $"{GetBaseApiUrl()}/ActivityStates";

    #endregion Base

    #region Methods

    public static ApiProviderHelper CreateActivityState()
        => new(HttpMethod.Post, RouteType.Body, GetActivityStatesBaseUrl());

    /// <summary>
    /// Every card in the column is moved to moveCardsToStateID before it is removed, so deleting
    /// a column can never strand a card.
    /// </summary>
    public static ApiProviderHelper DeleteActivityState(long activityStateID, long moveCardsToStateID)
        => new(HttpMethod.Delete, RouteType.Route, $"{GetActivityStateBaseUrl(activityStateID)}?moveCardsToStateID={moveCardsToStateID}");

    public static ApiProviderHelper GetActivityStates()
        => new(HttpMethod.Get, RouteType.Route, GetActivityStatesBaseUrl());

    public static ApiProviderHelper UpdateActivityState(long activityStateID)
        => new(HttpMethod.Patch, RouteType.Body, GetActivityStateBaseUrl(activityStateID));

    #endregion Methods

}
