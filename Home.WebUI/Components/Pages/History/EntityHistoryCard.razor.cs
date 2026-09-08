using Home.WebUI.DataAccess.History.GetEntityHistory;
using Home.WebUI.DataAccess.History.Models;
using Home.WebUI.Infrastructure.ApiProviders;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.History;

public partial class EntityHistoryCard
{

    #region Fields

    private List<HistoryEntryDto>? m_Entries;
    private int m_LoadedVersion = -1;
    private DateTime m_Now;
    private DateOnly m_Today;

    #endregion Fields

    #region Properties

    [CascadingParameter(Name = "CancellationToken")] public CancellationToken CancellationToken { get; set; }
    [Parameter] public long EntityID { get; set; }

    /// <summary>
    /// The API's value for the kind of thing: 2 a chore, 3 a recipe.
    /// </summary>
    [Parameter] public long ResourceTypeValue { get; set; }

    /// <summary>
    /// Bump to reload after the page saves something, so the card shows what was just done.
    /// </summary>
    [Parameter] public int Version { get; set; }

    #endregion Properties

    #region Lifecycle Methods

    protected override async Task OnParametersSetAsync()
    {
        if (this.m_LoadedVersion == this.Version || this.EntityID == 0)
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

        // Errors stay quiet: a page whose history will not load is still a page.
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetEntityHistoryWebAppResponse>(
            null!, ApiProvider.GetEntityHistory(this.ResourceTypeValue, this.EntityID),
            _ => { },
            this.CancellationToken);

        this.m_Entries = _Result?.Entries ?? [];
    }

    #endregion Methods

}
