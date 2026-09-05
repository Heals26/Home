namespace Home.WebUI.DataAccess.Devices.GetDevices;

/// <summary>
/// The devices currently signed in to this household.
/// </summary>
public class GetDevicesWebAppResponse
{

    #region Properties

    /// <summary>
    /// Most recently used first.
    /// </summary>
    public List<DeviceDto> Devices { get; set; } = [];

    #endregion Properties

}

/// <summary>
/// One signed-in device.
/// </summary>
public class DeviceDto
{

    #region Properties

    /// <summary>
    /// The session's ID, used to sign this device out.
    /// </summary>
    public long AuthenticationMetadataID { get; set; }

    /// <summary>
    /// Whether this is the device the screen is being read on. It cannot be signed out from here.
    /// </summary>
    public bool IsCurrentDevice { get; set; }

    /// <summary>
    /// When the session was last used, or null if it has not been used since signing in.
    /// </summary>
    public DateTime? LastUsedOnUTC { get; set; }

    /// <summary>
    /// What the device is called, worked out from the browser it signed in with.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// When this device signed in.
    /// </summary>
    public DateTime SignedInOnUTC { get; set; }

    #endregion Properties

}
