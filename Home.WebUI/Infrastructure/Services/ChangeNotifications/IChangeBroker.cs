namespace Home.WebUI.Infrastructure.Services.ChangeNotifications;

/// <summary>
/// The fan-out between a household's devices, relayed through the API's SignalR hub so it
/// stays correct when this app scales past one instance. One shared connection per household
/// per instance — never one per circuit — keeps TCP socket use flat however many pages open.
/// </summary>
public interface IChangeBroker
{

    #region Methods

    Task PublishAsync(long householdID, ChangeArea area, Func<Task<string?>> accessTokenProvider, CancellationToken cancellationToken);

    Task<IDisposable?> SubscribeAsync(long householdID, Func<ChangeArea, Task> handler, Func<Task<string?>> accessTokenProvider, CancellationToken cancellationToken);

    #endregion Methods

}
