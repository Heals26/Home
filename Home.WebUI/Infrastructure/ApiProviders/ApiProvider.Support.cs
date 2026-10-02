using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetFeedbackBaseUrl()
        => $"{GetBaseApiUrl()}/Support/Feedback";

    #endregion Base

    #region Methods

    public static ApiProviderHelper CreateFeedback()
        => new(HttpMethod.Post, RouteType.Body, GetFeedbackBaseUrl());

    #endregion Methods

}
