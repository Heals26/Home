using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Methods

    public static ApiProviderHelper UndoAction(Guid token)
        => new(HttpMethod.Post, RouteType.Route, $"{GetBaseApiUrl()}/Undo/{token}");

    #endregion Methods

}
