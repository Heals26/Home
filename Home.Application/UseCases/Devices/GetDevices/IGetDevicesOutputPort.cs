using Home.Domain.Entities;

namespace Home.Application.UseCases.Devices.GetDevices;

public interface IGetDevicesOutputPort
{

    #region Methods

    /// <summary>
    /// The household's live sessions, most recently used first.
    /// </summary>
    /// <param name="currentSessionID">
    /// The session this request arrived on, so one row can be marked as the device reading the
    /// screen. Null when the caller could not be placed, in which case no row is marked rather
    /// than the wrong one.
    /// </param>
    Task PresentDevicesAsync(IEnumerable<UserAuthentication> devices, long? currentSessionID, CancellationToken cancellationToken);

    #endregion Methods

}
