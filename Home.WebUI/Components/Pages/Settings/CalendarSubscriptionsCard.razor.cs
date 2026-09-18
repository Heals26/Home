using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.DataAccess.CalendarSubscriptions.CreateCalendarSubscription;
using Home.WebUI.DataAccess.CalendarSubscriptions.GetCalendarSubscriptions;
using Home.WebUI.DataAccess.CalendarSubscriptions.Models;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.CancellationTokens;
using Home.WebUI.Infrastructure.Services.ChangeNotifications;
using Home.WebUI.Infrastructure.Services.Undo;

namespace Home.WebUI.Components.Pages.Settings;

public partial class CalendarSubscriptionsCard : IDisposable
{

    #region Fields

    private CancellationTokenHandler m_CancellationTokenHandler = new();
    private IDisposable? m_ChangeSubscription;
    private ErrorHandler? m_ErrorHandler;

    // Loaded data
    private List<CalendarSubscriptionDto>? m_Subscriptions;

    // Adding
    private bool m_Adding;
    private string m_NewName = string.Empty;
    private string m_NewUrl = string.Empty;
    private string? m_Notice;

    // Per-row actions
    private long? m_RefreshingID;
    private long? m_RemovingID;

    #endregion Fields

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        // Warms the viewer's zone so the fetch times below are converted, not shown in UTC.
        _ = await this.ViewerClock.GetTimeZoneIDAsync(this.m_CancellationTokenHandler.Token);

        await this.LoadAsync();

        this.m_ChangeSubscription = await this.ChangeBroadcaster.SubscribeAsync(
            this.OnHouseholdChangedAsync, this.m_CancellationTokenHandler.Token);
    }

    public void Dispose()
    {
        this.m_ChangeSubscription?.Dispose();
        this.m_CancellationTokenHandler.Dispose();
    }

    #endregion Lifecycle Methods

    #region Methods

    /// <summary>
    /// A calendar put back with Undo, or added on another device, shows up without leaving the page.
    /// </summary>
    private async Task OnHouseholdChangedAsync(ChangeArea area)
    {
        if (area != ChangeArea.Calendar)
            return;

        await this.InvokeAsync(async () =>
        {
            await this.LoadAsync();
            this.StateHasChanged();
        });
    }

    private async Task LoadAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetCalendarSubscriptionsWebAppResponse>(
            null!, ApiProvider.GetCalendarSubscriptions(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != null)
            this.m_Subscriptions = _Result.Subscriptions;
    }

    private async Task AddAsync()
    {
        if (this.m_Adding || string.IsNullOrWhiteSpace(this.m_NewUrl))
            return;

        this.m_Adding = true;
        this.m_Notice = null;

        var _Result = await this.ApiAccess.SendRequestAsync<CreateCalendarSubscriptionWebAppRequest, CreateCalendarSubscriptionWebAppResponse>(
            new CreateCalendarSubscriptionWebAppRequest() { Name = this.m_NewName.Trim(), Url = this.m_NewUrl.Trim() },
            ApiProvider.CreateCalendarSubscription(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_Adding = false;

        if (_Result == null)
            return;

        this.m_NewName = string.Empty;
        this.m_NewUrl = string.Empty;

        if (!_Result.WasFetched)
            this.m_Notice = "Added, but it could not be read just now. Home will keep trying; check the address if it stays that way.";

        await this.LoadAsync();
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.Calendar, this.m_CancellationTokenHandler.Token);
    }

    private async Task RefreshAsync(CalendarSubscriptionDto subscription)
    {
        this.m_RefreshingID = subscription.CalendarSubscriptionID;

        var _Result = await this.ApiAccess.SendRequestAsync<object, bool>(
            null!, ApiProvider.RefreshCalendarSubscription(subscription.CalendarSubscriptionID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_RefreshingID = null;

        // Reloaded either way: a failed read is recorded on the row and the card should say so.
        await this.LoadAsync();

        if (_Result == true)
            await this.ChangeBroadcaster.PublishAsync(ChangeArea.Calendar, this.m_CancellationTokenHandler.Token);
    }

    private async Task RemoveAsync(CalendarSubscriptionDto subscription)
    {
        this.m_RemovingID = subscription.CalendarSubscriptionID;

        var _Result = await this.UndoLogic.SendRequestAsync<object, bool>(
            null!, ApiProvider.DeleteCalendarSubscription(subscription.CalendarSubscriptionID),
            new UndoOffer(ChangeArea.Calendar, $"Removed {subscription.Name}"),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_RemovingID = null;

        if (_Result != true)
            return;

        await this.LoadAsync();
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.Calendar, this.m_CancellationTokenHandler.Token);
    }

    private string Describe(CalendarSubscriptionDto subscription)
    {
        if (subscription.LastError != null)
            return $"Could not be read. {subscription.LastError}";

        if (subscription.LastFetchedUTC is { } _Fetched)
            return $"Updated {this.ViewerClock.ToLocal(_Fetched):h:mm tt, d MMM} · {subscription.Host}";

        return $"Not read yet · {subscription.Host}";
    }

    #endregion Methods

}
