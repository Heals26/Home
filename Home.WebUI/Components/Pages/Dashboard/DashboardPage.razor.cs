using Home.WebUI.DataAccess.Activities.GetActivities;
using Home.WebUI.DataAccess.Activities.Models;
using Home.WebUI.DataAccess.Lights.GetLights;
using Home.WebUI.DataAccess.Lights.Models;
using Home.WebUI.DataAccess.Recipes.GetRecipes;
using Home.WebUI.DataAccess.ShoppingLists.GetShoppingLists;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.CancellationTokens;

namespace Home.WebUI.Components.Pages.Dashboard;

public partial class DashboardPage
{

    #region Fields

    private CancellationTokenHandler m_CancellationTokenHandler = new();
    private ICollection<ActivitySummaryDto>? m_Activities;
    private ICollection<LightGroupDto>? m_LightGroups;
    private ICollection<GetRecipeDto>? m_Recipes;
    private ICollection<GetShoppingListDto>? m_ShoppingLists;
    private bool m_LoadFailed;

    #endregion Fields

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
        => await Task.WhenAll(
            this.LoadActivitiesAsync(),
            this.LoadLightsAsync(),
            this.LoadRecipesAsync(),
            this.LoadShoppingListsAsync());

    #endregion Lifecycle Methods

    #region Methods

    private string GetGreeting()
    {
        var _Hour = this.TimeProvider.GetLocalNow().Hour;

        if (_Hour < 12)
            return "morning";

        if (_Hour < 17)
            return "afternoon";

        return "evening";
    }

    // The board never toasts — a failed tile degrades to its empty state and the
    // shared banner explains once.
    private async Task LoadActivitiesAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetActivitiesWebAppResponse>(
            null!, ApiProvider.GetActivities(),
            _ => this.m_LoadFailed = true,
            this.m_CancellationTokenHandler.Token);

        this.m_Activities = _Result?.Activities ?? [];
    }

    private async Task LoadLightsAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetLightsWebAppResponse>(
            null!, ApiProvider.GetLights(),
            _ => { },
            this.m_CancellationTokenHandler.Token);

        this.m_LightGroups = _Result?.Groups ?? [];
    }

    private async Task LoadRecipesAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetRecipesWebAppResponse>(
            null!, ApiProvider.GetRecipes(),
            _ => this.m_LoadFailed = true,
            this.m_CancellationTokenHandler.Token);

        this.m_Recipes = _Result?.Recipes ?? [];
    }

    private async Task LoadShoppingListsAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetShoppingListsWebAppResponse>(
            null!, ApiProvider.GetShoppingLists(),
            _ => this.m_LoadFailed = true,
            this.m_CancellationTokenHandler.Token);

        this.m_ShoppingLists = _Result?.ShoppingLists ?? [];
    }

    private IEnumerable<ActivitySummaryDto> UpcomingActivities()
        => (this.m_Activities ?? [])
            .Where(a => a.CompletedDateUTC == null)
            .OrderBy(a => a.DueDateUTC ?? DateTime.MaxValue)
            .Take(4);

    private static string DayChip(ActivitySummaryDto activity, DateTime today)
    {
        if (activity.DueDateUTC == null)
            return "—";

        var _Due = activity.DueDateUTC.Value.ToLocalTime().Date;

        if (_Due == today)
            return "Today";

        if (_Due == today.AddDays(1))
            return "Tmrw";

        return _Due < today ? "Late" : _Due.ToString("ddd");
    }

    private string DayChipClasses(ActivitySummaryDto activity)
    {
        var _Chip = DayChip(activity, this.TimeProvider.GetLocalNow().Date);

        return _Chip switch
        {
            "Late" => "bg-red-500/10 text-red-300",
            "Today" => "bg-week/10 text-week",
            _ => "bg-ink-800 text-ink-300"
        };
    }

    #endregion Methods

}
