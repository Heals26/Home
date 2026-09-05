namespace Home.Application.UseCases.Devices.SignOutOtherDevices;

public interface ISignOutOtherDevicesOutputPort
{

    #region Methods

    /// <summary>
    /// The caller could not be placed to a session, so there is no "everything else" to end
    /// without also ending this one. Refused rather than guessed at.
    /// </summary>
    Task PresentCurrentDeviceUnknownAsync(CancellationToken cancellationToken);

    Task PresentOtherDevicesSignedOutAsync(int signedOutCount, CancellationToken cancellationToken);

    #endregion Methods

}
