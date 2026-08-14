using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetAnnouncementBaseUrl(long announcementID)
        => $"{GetAnnouncementsBaseUrl()}/{announcementID}";

    private static string GetAnnouncementsBaseUrl()
        => $"{GetBaseApiUrl()}/Announcements";

    #endregion Base

    #region Methods

    public static ApiProviderHelper CreateAnnouncement()
        => new(HttpMethod.Post, RouteType.Body, GetAnnouncementsBaseUrl());

    public static ApiProviderHelper DeleteAnnouncement(long announcementID)
        => new(HttpMethod.Delete, RouteType.Route, GetAnnouncementBaseUrl(announcementID));

    public static ApiProviderHelper GetAnnouncements()
        => new(HttpMethod.Get, RouteType.Route, GetAnnouncementsBaseUrl());

    #endregion Methods

}
