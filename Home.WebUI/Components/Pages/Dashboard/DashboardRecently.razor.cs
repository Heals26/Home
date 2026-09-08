using Home.WebUI.DataAccess.History.GetHistory;
using Home.WebUI.DataAccess.History.Models;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.History;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Dashboard;

public partial class DashboardRecently
{

    #region Constants

    private const int Rows = 4;

    #endregion Constants

    #region Fields

    private List<HistoryEntryDto>? m_Entries;
    private int m_LoadedVersion = -1;
    private DateTime m_Now;
    private DateOnly m_Today;

    #endregion Fields

    #region Properties

    [CascadingParameter(Name = "CancellationToken")] public CancellationToken CancellationToken { get; set; }

    /// <summary>
    /// Bumped by the board whenever anything in the household changes, which is exactly when
    /// there is something new to show here.
    /// </summary>
    [Parameter] public int Version { get; set; }

    #endregion Properties

    #region Lifecycle Methods

    protected override async Task OnParametersSetAsync()
    {
        if (this.m_LoadedVersion == this.Version)
            return;

        this.m_LoadedVersion = this.Version;

        await this.LoadAsync();
    }

    #endregion Lifecycle Methods

    #region Methods

    private async Task LoadAsync()
    {
        this.m_Today = await this.ViewerClock.TodayAsync(this.CancellationToken);
        this.m_Now = this.ViewerClock.ToLocal(this.TimeProvider.GetUtcNow().UtcDateTime);

        var _Categories = HistoryFormat.ParseCategories(await this.DevicePreferences.GetAsync(HistoryFormat.PreferenceKey, this.CancellationToken));

        // The board never toasts; an empty tile is the failure state.
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetHistoryWebAppResponse>(
            null!, ApiProvider.GetHistory(Rows, _Categories),
            _ => { },
            this.CancellationToken);

        this.m_Entries = _Result?.Entries ?? [];
    }

    #endregion Methods

}
