using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.DataAccess.History.GetHistory;
using Home.WebUI.DataAccess.History.Models;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.CancellationTokens;
using Home.WebUI.Infrastructure.History;
using Home.WebUI.Infrastructure.Services.ChangeNotifications;

namespace Home.WebUI.Components.Pages.History;

public partial class HistoryPage : IDisposable
{

    #region Constants

    private const int PageSize = 40;

    #endregion Constants

    #region Fields

    private CancellationTokenHandler m_CancellationTokenHandler = new();
    private ErrorHandler? m_ErrorHandler;
    private IDisposable? m_ChangeSubscription;

    // What is on show
    private HashSet<HistoryCategory> m_Categories = [.. HistoryFormat.DefaultCategories];
    private List<HistoryEntryDto>? m_Entries;
    private bool m_HasMore;
    private bool m_LoadingMore;
    private DateTime m_Now;
    private DateOnly m_Today;

    #endregion Fields

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        this.m_Today = await this.ViewerClock.TodayAsync(this.m_CancellationTokenHandler.Token);
        this.m_Now = this.ViewerClock.ToLocal(this.TimeProvider.GetUtcNow().UtcDateTime);
        this.m_Categories = [.. HistoryFormat.ParseCategories(await this.DevicePreferences.GetAsync(HistoryFormat.PreferenceKey, this.m_CancellationTokenHandler.Token))];

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
    /// Anything the household does is history, so any change reloads the top of the feed.
    /// </summary>
    private async Task OnHouseholdChangedAsync(ChangeArea area)
        => await this.InvokeAsync(async () =>
        {
            await this.LoadAsync();
            this.StateHasChanged();
        });

    private async Task LoadAsync()
    {
        if (this.m_Categories.Count == 0)
        {
            this.m_Entries = [];
            this.m_HasMore = false;
            return;
        }

        var _Result = await this.ApiAccess.SendRequestAsync<object, GetHistoryWebAppResponse>(
            null!, ApiProvider.GetHistory(PageSize, this.m_Categories),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result == null)
            return;

        this.m_Now = this.ViewerClock.ToLocal(this.TimeProvider.GetUtcNow().UtcDateTime);
        this.m_Entries = _Result.Entries;
        this.m_HasMore = _Result.HasMore;
    }

    private async Task LoadMoreAsync()
    {
        if (this.m_LoadingMore || this.m_Entries is not { Count: > 0 } _Entries)
            return;

        this.m_LoadingMore = true;

        var _Result = await this.ApiAccess.SendRequestAsync<object, GetHistoryWebAppResponse>(
            null!, ApiProvider.GetHistory(PageSize, this.m_Categories, _Entries[^1].AuditID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_LoadingMore = false;

        if (_Result == null)
            return;

        _Entries.AddRange(_Result.Entries);
        this.m_HasMore = _Result.HasMore;
    }

    private async Task ToggleCategoryAsync(HistoryCategory category)
    {
        if (!this.m_Categories.Remove(category))
            _ = this.m_Categories.Add(category);

        await this.DevicePreferences.SetAsync(HistoryFormat.PreferenceKey, HistoryFormat.SerialiseCategories(this.m_Categories), this.m_CancellationTokenHandler.Token);

        this.m_Entries = null;

        await this.LoadAsync();
    }

    #endregion Methods

}
