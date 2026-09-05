namespace Home.WebUI.DataAccess.Devices.SignOutOtherDevices;

/// <summary>
/// The result of signing out everywhere except this device.
/// </summary>
public class SignOutOtherDevicesWebAppResponse
{

    #region Properties

    /// <summary>
    /// How many devices were signed out.
    /// </summary>
    public int SignedOutCount { get; set; }

    #endregion Properties

}
