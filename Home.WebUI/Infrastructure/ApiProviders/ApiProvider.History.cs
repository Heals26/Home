using Home.WebUI.DataAccess.History.Models;
using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetHistoryBaseUrl()
        => $"{GetBaseApiUrl()}/History";

    #endregion Base

    #region Methods

    /// <summary>
    /// Everything that happened to one thing. The resource type values are the API's: 2 a chore,
    /// 3 a recipe.
    /// </summary>
    public static ApiProviderHelper GetEntityHistory(long resourceTypeValue, long entityID)
        => new(HttpMethod.Get, RouteType.Route, $"{GetHistoryBaseUrl()}/{resourceTypeValue}/{entityID}");

    /// <summary>
    /// A page of the household's feed. No categories means everything.
    /// </summary>
    public static ApiProviderHelper GetHistory(int take, IEnumerable<HistoryCategory> categories, long? beforeAuditID = null)
    {
        var _Query = new List<string> { $"take={take}" };

        if (beforeAuditID != null)
            _Query.Add($"beforeAuditID={beforeAuditID}");

        _Query.AddRange(categories.Select(c => $"categories={(int)c}"));

        return new(HttpMethod.Get, RouteType.Route, $"{GetHistoryBaseUrl()}?{string.Join("&", _Query)}");
    }

    #endregion Methods

}
