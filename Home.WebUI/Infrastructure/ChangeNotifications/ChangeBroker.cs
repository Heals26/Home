using Home.WebUI.Infrastructure.Services.ChangeNotifications;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Concurrent;

namespace Home.WebUI.Infrastructure.ChangeNotifications;

/// <summary>
/// Multiplexes every circuit on this instance onto one hub connection per household. All
/// circuits run server-side, so these are server-to-server sockets — the devices carry
/// nothing extra. WebSockets only: the long-polling fallback is what historically chewed
/// through TCP sockets, so it is deliberately off the table.
/// </summary>
public class ChangeBroker(IConfiguration configuration, ILogger<ChangeBroker> logger) : IChangeBroker, IAsyncDisposable
{

    #region Fields

    private readonly SemaphoreSlim m_ConnectLock = new(1, 1);
    private readonly ConcurrentDictionary<long, HouseholdConnection> m_Households = new();

    #endregion Fields

    #region Methods

    public async ValueTask DisposeAsync()
    {
        foreach (var _Household in this.m_Households.Values)
            await _Household.Connection.DisposeAsync();

        this.m_Households.Clear();
    }

    async Task IChangeBroker.PublishAsync(long householdID, ChangeArea area, Func<Task<string?>> accessTokenProvider, CancellationToken cancellationToken)
    {
        var _Household = await this.GetOrConnectAsync(householdID, accessTokenProvider, cancellationToken);

        if (_Household == null)
            return;

        try
        {
            await _Household.Connection.InvokeAsync("PublishAsync", area.ToString(), cancellationToken);
        }
        catch (Exception _Exception)
        {
            // The publisher already updated its own screen; the others catch up on their next load.
            logger.LogWarning(_Exception, "Publishing a {Area} change did not reach the hub.", area);
        }
    }

    async Task<IDisposable?> IChangeBroker.SubscribeAsync(long householdID, Func<ChangeArea, Task> handler, Func<Task<string?>> accessTokenProvider, CancellationToken cancellationToken)
    {
        var _Household = await this.GetOrConnectAsync(householdID, accessTokenProvider, cancellationToken);

        if (_Household == null)
            return null;

        var _Key = Guid.NewGuid();
        _Household.Handlers[_Key] = handler;

        return new SubscriptionToken(() =>
        {
            _ = _Household.Handlers.TryRemove(_Key, out _);

            // The last page leaving closes the socket rather than holding it open forever.
            if (_Household.Handlers.IsEmpty && this.m_Households.TryRemove(householdID, out var _Removed))
                _ = _Removed.Connection.DisposeAsync();
        });
    }

    private async Task<HouseholdConnection?> GetOrConnectAsync(long householdID, Func<Task<string?>> accessTokenProvider, CancellationToken cancellationToken)
    {
        if (this.m_Households.TryGetValue(householdID, out var _Existing))
        {
            _Existing.AccessTokenProvider = accessTokenProvider;
            return _Existing;
        }

        await this.m_ConnectLock.WaitAsync(cancellationToken);

        try
        {
            if (this.m_Households.TryGetValue(householdID, out _Existing))
            {
                _Existing.AccessTokenProvider = accessTokenProvider;
                return _Existing;
            }

            var _Household = new HouseholdConnection() { AccessTokenProvider = accessTokenProvider };

            var _Connection = new HubConnectionBuilder()
                .WithUrl(BuildHubUri(configuration["apiBaseUrl"]), HttpTransportType.WebSockets, o =>
                {
                    o.AccessTokenProvider = () => _Household.AccessTokenProvider();
                })
                .WithAutomaticReconnect()
                .Build();

            _ = _Connection.On<string>("Changed", async area =>
            {
                if (!Enum.TryParse<ChangeArea>(area, out var _Area))
                    return;

                foreach (var _Handler in _Household.Handlers.Values.ToArray())
                {
                    try
                    {
                        await _Handler(_Area);
                    }
                    catch (Exception _Exception)
                    {
                        logger.LogDebug(_Exception, "A change handler failed; the other circuits were still told.");
                    }
                }
            });

            await _Connection.StartAsync(cancellationToken);

            _Household.Connection = _Connection;
            this.m_Households[householdID] = _Household;

            return _Household;
        }
        catch (Exception _Exception)
        {
            logger.LogWarning(_Exception, "Could not reach the change hub; live updates are off until the next attempt.");
            return null;
        }
        finally
        {
            _ = this.m_ConnectLock.Release();
        }
    }

    private static Uri BuildHubUri(string? apiBaseUrl)
        => new(new Uri(apiBaseUrl!), "/hubs/changes");

    #endregion Methods

    #region Nested Types

    private sealed class HouseholdConnection
    {
        /// <summary>
        /// Refreshed on every subscribe so reconnects always use a live circuit's token, not
        /// one belonging to a circuit that has since gone away.
        /// </summary>
        public volatile Func<Task<string?>> AccessTokenProvider = () => Task.FromResult<string?>(null);
        public HubConnection Connection = null!;
        public ConcurrentDictionary<Guid, Func<ChangeArea, Task>> Handlers { get; } = new();
    }

    private sealed class SubscriptionToken(Action unsubscribe) : IDisposable
    {
        private int m_Disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref this.m_Disposed, 1) == 0)
                unsubscribe();
        }
    }

    #endregion Nested Types

}
