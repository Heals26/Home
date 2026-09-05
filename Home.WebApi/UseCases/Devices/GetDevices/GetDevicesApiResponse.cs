namespace Home.WebApi.UseCases.Devices.GetDevices;

public class GetDevicesApiResponse
{

    #region Properties

    public ICollection<DeviceDto> Devices { get; set; } = [];

    #endregion Properties

}

public class DeviceDto
{

    #region Properties

    public long AuthenticationMetadataID { get; set; }

    /// <summary>
    /// Whether this is the device the screen is being read on. Exactly one row carries it, or none
    /// when the caller could not be placed.
    /// </summary>
    public bool IsCurrentDevice { get; set; }

    /// <summary>
    /// What the family would recognise the device as, worked out from the User-Agent at sign-in.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    public DateTime SignedInOnUTC { get; set; }

    /// <summary>
    /// When the session last did anything, or null if it has not been used since sign-in.
    /// </summary>
    public DateTime? LastUsedOnUTC { get; set; }

    #endregion Properties

}
