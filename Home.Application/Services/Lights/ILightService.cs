namespace Home.Application.Services.Lights;

/// <summary>
/// The boundary to whatever actually drives the bulbs. Implemented in the outer layer so the
/// use cases never know which vendor is on the other end.
/// </summary>
public interface ILightService
{

    #region Methods

    /// <summary>
    /// Every light on the account with its current state, or null if the provider could not be
    /// reached. Implementations must not throw for an unreachable provider — an offline hub is a
    /// normal Tuesday, not an exception.
    /// </summary>
    Task<IReadOnlyList<LightSnapshot>?> GetLightsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Applies one partial state change to many lights at once. Implementations should address the
    /// whole set in as few provider calls as possible rather than looping per light.
    /// </summary>
    Task<LightCommandResult> SetGroupStateAsync(
        IReadOnlyCollection<string> lightIDs,
        LightStateChange change,
        CancellationToken cancellationToken);

    /// <summary>
    /// Applies a partial state change to one light.
    /// </summary>
    Task<LightCommandResult> SetStateAsync(string lightID, LightStateChange change, CancellationToken cancellationToken);

    #endregion Methods

}
