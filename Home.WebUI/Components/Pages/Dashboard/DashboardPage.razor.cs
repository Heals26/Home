using Home.WebUI.DataAccess.Activities.GetActivities;
using Home.WebUI.DataAccess.Activities.Models;
using Home.WebUI.DataAccess.Announcements.CreateAnnouncement;
using Home.WebUI.DataAccess.Announcements.GetAnnouncements;
using Home.WebUI.DataAccess.Announcements.Models;
using Microsoft.AspNetCore.Components.Web;
using Home.WebUI.DataAccess.Lights.GetLights;
using Home.WebUI.DataAccess.Lights.Models;
using Home.WebUI.DataAccess.MealPlanEntries.GetMealPlanEntries;
using Home.WebUI.DataAccess.MealPlanEntries.Models;
using Home.WebUI.DataAccess.Recipes.GetRecipes;
using Home.WebUI.DataAccess.ShoppingLists.GetShoppingLists;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.CancellationTokens;
using Home.WebUI.Infrastructure.Services.ChangeNotifications;

namespace Home.WebUI.Components.Pages.Dashboard;

public partial class DashboardPage : IDisposable
{

    #region Fields

    private CancellationTokenHandler m_CancellationTokenHandler = new();
    private IDisposable? m_ChangeSubscription;
    private ICollection<ActivitySummaryDto>? m_Activities;
    private ICollection<AnnouncementDto>? m_Announcements;
    private ICollection<LightGroupDto>? m_LightGroups;
    private ICollection<MealPlanEntryDto>? m_MealPlanEntries;
    private ICollection<GetRecipeDto>? m_Recipes;
    private ICollection<GetShoppingListDto>? m_ShoppingLists;
    private bool m_LoadFailed;

    // Family notes
    private string m_NewAnnouncement = string.Empty;
    private bool m_PostingAnnouncement;

    // Live changes arrive by push; this slow sweep only covers a hub outage so the board
    // can never sit stale for long on an always-on screen.
    private static readonly TimeSpan s_RefreshInterval = TimeSpan.FromMinutes(5);

    #endregion Fields

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        await this.LoadEverythingAsync();

        this.m_ChangeSubscription = await this.ChangeBroadcaster.SubscribeAsync(
            this.OnHouseholdChangedAsync, this.m_CancellationTokenHandler.Token);

        _ = this.RefreshLoopAsync();
    }

    public void Dispose()
    {
        this.m_ChangeSubscription?.Dispose();
        this.m_CancellationTokenHandler.Dispose();
    }

    #endregion Lifecycle Methods

    #region Methods

    private async Task LoadEverythingAsync()
        => await Task.WhenAll(
            this.LoadActivitiesAsync(),
            this.LoadAnnouncementsAsync(),
            this.LoadLightsAsync(),
            this.LoadMealPlanAsync(),
            this.LoadRecipesAsync(),
            this.LoadShoppingListsAsync());

    private async Task RefreshLoopAsync()
    {
        try
        {
            using var _Timer = new PeriodicTimer(s_RefreshInterval, this.TimeProvider);

            while (await _Timer.WaitForNextTickAsync(this.m_CancellationTokenHandler.Token))
            {
                await this.LoadEverythingAsync();
                await this.InvokeAsync(this.StateHasChanged);
            }
        }
        catch (OperationCanceledException)
        {
            // Navigating away cancels the token — the loop simply ends with the page.
        }
    }

    private async Task OnHouseholdChangedAsync(ChangeArea area)
        => await this.InvokeAsync(async () =>
        {
            var _Load = area switch
            {
                ChangeArea.Activities or ChangeArea.Users => this.LoadActivitiesAsync(),
                ChangeArea.Announcements => this.LoadAnnouncementsAsync(),
                ChangeArea.Lights => this.LoadLightsAsync(),
                ChangeArea.MealPlan => this.LoadMealPlanAsync(),
                ChangeArea.Recipes => Task.WhenAll(this.LoadRecipesAsync(), this.LoadMealPlanAsync()),
                ChangeArea.ShoppingLists => this.LoadShoppingListsAsync(),
                _ => Task.CompletedTask
            };

            await _Load;
            this.StateHasChanged();
        });

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

    private async Task LoadAnnouncementsAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetAnnouncementsWebAppResponse>(
            null!, ApiProvider.GetAnnouncements(),
            _ => { },
            this.m_CancellationTokenHandler.Token);

        this.m_Announcements = _Result?.Announcements ?? [];
    }

    private async Task LoadLightsAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetLightsWebAppResponse>(
            null!, ApiProvider.GetLights(),
            _ => { },
            this.m_CancellationTokenHandler.Token);

        this.m_LightGroups = _Result?.Groups ?? [];
    }

    private async Task LoadMealPlanAsync()
    {
        var _Today = this.TimeProvider.GetLocalNow().Date;

        var _Result = await this.ApiAccess.SendRequestAsync<object, GetMealPlanEntriesWebAppResponse>(
            null!, ApiProvider.GetMealPlanEntries(_Today, _Today.AddDays(1)),
            _ => { },
            this.m_CancellationTokenHandler.Token);

        this.m_MealPlanEntries = _Result?.Entries ?? [];
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

    private async Task OnAnnouncementKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
            await this.PostAnnouncementAsync();
    }

    private async Task PostAnnouncementAsync()
    {
        if (this.m_PostingAnnouncement || string.IsNullOrWhiteSpace(this.m_NewAnnouncement))
            return;

        this.m_PostingAnnouncement = true;

        var _Result = await this.ApiAccess.SendRequestAsync<CreateAnnouncementWebAppRequest, CreateAnnouncementWebAppResponse>(
            new CreateAnnouncementWebAppRequest() { Content = this.m_NewAnnouncement.Trim() },
            ApiProvider.CreateAnnouncement(),
            _ => this.m_LoadFailed = true,
            this.m_CancellationTokenHandler.Token);

        this.m_PostingAnnouncement = false;

        if (_Result == null)
            return;

        this.m_NewAnnouncement = string.Empty;

        await this.LoadAnnouncementsAsync();
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.Announcements, this.m_CancellationTokenHandler.Token);
    }

    private async Task DeleteAnnouncementAsync(AnnouncementDto announcement)
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, bool>(
            null!, ApiProvider.DeleteAnnouncement(announcement.AnnouncementID),
            _ => this.m_LoadFailed = true,
            this.m_CancellationTokenHandler.Token);

        if (_Result != true)
            return;

        _ = this.m_Announcements?.Remove(announcement);
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.Announcements, this.m_CancellationTokenHandler.Token);
    }

    private IEnumerable<MealPlanEntryDto> TonightsMeals()
        => (this.m_MealPlanEntries ?? []).Where(e => e.Date.Date == this.TimeProvider.GetLocalNow().Date);

    private IEnumerable<MealPlanEntryDto> TomorrowsMeals()
        => (this.m_MealPlanEntries ?? []).Where(e => e.Date.Date == this.TimeProvider.GetLocalNow().Date.AddDays(1));

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
