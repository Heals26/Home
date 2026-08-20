using Home.WebUI.DataAccess.Households.GetHouseholdSettings;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.Services.ChangeNotifications;
using Home.WebUI.Infrastructure.Services.HttpClients;
using Home.WebUI.Infrastructure.Services.Security;

namespace Home.WebUI.Infrastructure.ChangeNotifications;

public class ChangeBroadcaster(
    IChangeBroker changeBroker,
    IHomeHttpClient apiAccess,
    IHouseholdSession householdSession)
    : IChangeBroadcaster
{

    #region Fields

    private long? m_HouseholdID;

    #endregion Fields

    #region Methods

    async Task IChangeBroadcaster.PublishAsync(ChangeArea area, CancellationToken cancellationToken)
    {
        var _HouseholdID = await this.ResolveHouseholdIDAsync(cancellationToken);

        if (_HouseholdID != null)
            await changeBroker.PublishAsync(_HouseholdID.Value, area, this.GetAccessTokenAsync, cancellationToken);
    }

    async Task<IDisposable?> IChangeBroadcaster.SubscribeAsync(Func<ChangeArea, Task> handler, CancellationToken cancellationToken)
    {
        var _HouseholdID = await this.ResolveHouseholdIDAsync(cancellationToken);

        return _HouseholdID == null
            ? null
            : await changeBroker.SubscribeAsync(_HouseholdID.Value, handler, this.GetAccessTokenAsync, cancellationToken);
    }

    private Task<string?> GetAccessTokenAsync()
        => householdSession.GetAccessTokenAsync(CancellationToken.None);

    /// <summary>
    /// The household comes from the API using the caller's own token, so a device can only
    /// ever publish to or hear about its own household. Resolved once per circuit.
    /// </summary>
    private async Task<long?> ResolveHouseholdIDAsync(CancellationToken cancellationToken)
    {
        if (this.m_HouseholdID != null)
            return this.m_HouseholdID;

        var _Settings = await apiAccess.SendRequestAsync<object, GetHouseholdSettingsWebAppResponse>(
            null!, ApiProvider.GetHouseholdSettings(), _ => { }, cancellationToken);

        if (_Settings?.HouseholdID > 0)
            this.m_HouseholdID = _Settings.HouseholdID;

        return this.m_HouseholdID;
    }

    #endregion Methods

}
