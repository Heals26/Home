namespace Home.WebUI.Infrastructure.Services.ChangeNotifications;

/// <summary>
/// The per-circuit face of the change broker: it resolves the signed-in household once and
/// keeps pages out of the business of knowing household IDs.
/// </summary>
public interface IChangeBroadcaster
{

    #region Methods

    Task PublishAsync(ChangeArea area, CancellationToken cancellationToken);

    /// <summary>
    /// Null when the household cannot be resolved (not signed in) — the page simply
    /// misses live updates rather than failing.
    /// </summary>
    Task<IDisposable?> SubscribeAsync(Func<ChangeArea, Task> handler, CancellationToken cancellationToken);

    #endregion Methods

}
