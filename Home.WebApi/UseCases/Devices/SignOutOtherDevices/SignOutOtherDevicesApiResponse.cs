namespace Home.WebApi.UseCases.Devices.SignOutOtherDevices;

public class SignOutOtherDevicesApiResponse
{

    #region Properties

    /// <summary>
    /// How many sessions were ended, so the screen can say what it just did.
    /// </summary>
    public int SignedOutCount { get; set; }

    #endregion Properties

}
