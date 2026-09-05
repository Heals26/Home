using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetDeviceBaseUrl(long authenticationMetadataID)
        => $"{GetDevicesBaseUrl()}/{authenticationMetadataID}";

    private static string GetDevicesBaseUrl()
        => $"{GetBaseApiUrl()}/Devices";

    private static string GetOtherDevicesBaseUrl()
        => $"{GetDevicesBaseUrl()}/others";

    #endregion Base

    #region Methods

    public static ApiProviderHelper GetDevices()
        => new(HttpMethod.Get, RouteType.Route, GetDevicesBaseUrl());

    public static ApiProviderHelper SignOutDevice(long authenticationMetadataID)
        => new(HttpMethod.Delete, RouteType.Route, GetDeviceBaseUrl(authenticationMetadataID));

    public static ApiProviderHelper SignOutOtherDevices()
        => new(HttpMethod.Delete, RouteType.Route, GetOtherDevicesBaseUrl());

    #endregion Methods

}
