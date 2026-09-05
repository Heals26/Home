namespace Home.Application.UseCases.Devices.SignOutDevice;

public interface ISignOutDeviceOutputPort
{

    #region Methods

    /// <summary>
    /// The caller asked to sign out the device it is reading the screen on. Refused, because the
    /// browser would keep a cookie for a session that no longer exists and only find out at the
    /// next refresh. Signing this device out is what the Sign out button is for.
    /// </summary>
    Task PresentCannotSignOutThisDeviceAsync(CancellationToken cancellationToken);

    Task PresentDeviceNotFoundAsync(long authenticationMetadataID, CancellationToken cancellationToken);
    Task PresentDeviceSignedOutAsync(CancellationToken cancellationToken);

    #endregion Methods

}
