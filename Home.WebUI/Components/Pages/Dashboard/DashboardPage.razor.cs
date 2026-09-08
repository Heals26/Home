using Home.WebUI.DataAccess.Activities.SetActivityCompletion;
using Home.WebUI.DataAccess.Announcements.CreateAnnouncement;
using Home.WebUI.DataAccess.Announcements.GetAnnouncements;
using Home.WebUI.DataAccess.Announcements.Models;
using Home.WebUI.DataAccess.Calendar.GetCalendar;
using Home.WebUI.DataAccess.Calendar.Models;
using Microsoft.AspNetCore.Components.Web;
using Home.WebUI.DataAccess.Lights.GetLights;
using Home.WebUI.DataAccess.Lights.Models;
using Home.WebUI.DataAccess.Recipes.GetRecipes;
using Home.WebUI.DataAccess.ShoppingLists.GetShoppingLists;
using Home.WebUI.DataAccess.Weather.GetWeather;
using Home.WebUI.DataAccess.Weather.Models;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.CancellationTokens;
using Home.WebUI.Infrastructure.Services.ChangeNotifications;

namespace Home.WebUI.Components.Pages.Dashboard;

public partial class DashboardPage : IDisposable
{

    #region Records

    private sealed record MealSlotGroup(string Name, string Meals);

    #endregion Records

    #region Fields

    private CancellationTokenHandler m_CancellationTokenHandler = new();
    private IDisposable? m_ChangeSubscription;
    private ICollection<AnnouncementDto>? m_Announcements;
    private ICollection<LightGroupDto>? m_LightGroups;
    private List<CalendarDayDto>? m_Calendar;
    private DateOnly m_Today;
    private string m_TimeZoneID = "UTC";
    private ICollection<GetRecipeDto>? m_Recipes;
    private ICollection<GetShoppingListDto>? m_ShoppingLists;
    private GetWeatherWebAppResponse? m_Weather;
    private bool m_LoadFailed;
    private bool m_WeatherFailed;

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
        _ = this.ClockLoopAsync();
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
            this.LoadCalendarAsync(),
            this.LoadAnnouncementsAsync(),
            this.LoadLightsAsync(),
            this.LoadRecipesAsync(),
            this.LoadShoppingListsAsync(),
            this.LoadWeatherAsync());

    /// <summary>
    /// Keeps the header clock honest. Separate from the refresh loop because this only redraws
    /// what is already on screen: asking the API for everything once a minute to move a clock hand
    /// would be seven calls a minute, all day, on a screen that is never turned off.
    /// <para>
    /// It waits out the current minute first so the display changes when the minute does, rather
    /// than up to 59 seconds late and staying that way.
    /// </para>
    /// </summary>
    private async Task ClockLoopAsync()
    {
        try
        {
            var _ToNextMinute = TimeSpan.FromSeconds(60 - this.TimeProvider.GetLocalNow().Second);

            await Task.Delay(_ToNextMinute, this.TimeProvider, this.m_CancellationTokenHandler.Token);
            await this.InvokeAsync(this.StateHasChanged);

            using var _Timer = new PeriodicTimer(TimeSpan.FromMinutes(1), this.TimeProvider);

            while (await _Timer.WaitForNextTickAsync(this.m_CancellationTokenHandler.Token))
                await this.InvokeAsync(this.StateHasChanged);
        }
        catch (OperationCanceledException)
        {
            // Navigating away cancels the token — the loop simply ends with the page.
        }
    }

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
                ChangeArea.Activities or ChangeArea.Calendar or ChangeArea.MealPlan or ChangeArea.Users => this.LoadCalendarAsync(),
                ChangeArea.Announcements => this.LoadAnnouncementsAsync(),
                ChangeArea.Lights => this.LoadLightsAsync(),
                ChangeArea.Recipes => Task.WhenAll(this.LoadRecipesAsync(), this.LoadCalendarAsync()),
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

    /// <summary>
    /// Today and the three days after it, placed on the viewer's own days. One read feeds both the
    /// agenda tile and the meals tile (see the 8 Sep 2026 decision on the dashboard).
    /// </summary>
    private async Task LoadCalendarAsync()
    {
        this.m_TimeZoneID = await this.ViewerClock.GetTimeZoneIDAsync(this.m_CancellationTokenHandler.Token);
        this.m_Today = await this.ViewerClock.TodayAsync(this.m_CancellationTokenHandler.Token);

        var _Result = await this.ApiAccess.SendRequestAsync<object, GetCalendarWebAppResponse>(
            null!, ApiProvider.GetCalendar(this.m_Today, this.m_Today.AddDays(3), this.m_TimeZoneID),
            _ => this.m_LoadFailed = true,
            this.m_CancellationTokenHandler.Token);

        this.m_Calendar = _Result?.Days ?? [];
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

    // Weather rides the same five-minute sweep as everything else; the forecaster is cached
    // server-side, so asking more often would only cost the tablet a request.
    private async Task LoadWeatherAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetWeatherWebAppResponse>(
            null!, ApiProvider.GetWeather(),
            _ => { },
            this.m_CancellationTokenHandler.Token);

        this.m_WeatherFailed = _Result == null;
        this.m_Weather = _Result ?? this.m_Weather;
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

    /// <summary>
    /// The tile names the meal rather than assuming everything planned today is dinner. Meals come
    /// off the calendar read already in the day's order and are grouped here by the meal they are.
    /// </summary>
    private List<MealSlotGroup> TodaysMealsBySlot()
        => [.. this.MealsOn(this.m_Today)
            .GroupBy(m => string.IsNullOrWhiteSpace(m.Subtitle) ? "Planned" : m.Subtitle)
            .Select(g => new MealSlotGroup(g.Key, string.Join(" · ", g.Select(m => m.Title))))];

    private IEnumerable<CalendarItemDto> TomorrowsMeals()
        => this.MealsOn(this.m_Today.AddDays(1));

    private IEnumerable<CalendarItemDto> MealsOn(DateOnly date)
        => (this.m_Calendar ?? []).Where(d => d.Date == date).SelectMany(d => d.Items).Where(i => i.Kind == CalendarItemKind.Meal);

    // Today's row leads the tile on its own, so the strip starts at tomorrow.
    private WeatherDayDto? Today()
        => this.m_Weather?.Forecast.FirstOrDefault();

    private IEnumerable<WeatherDayDto> ForecastStrip()
        => (this.m_Weather?.Forecast ?? []).Skip(1).Take(3);

    private static string Temperature(double celsius)
        => $"{(int)Math.Round(celsius)}°";

    private async Task CompleteActivityAsync(CalendarItemDto item)
    {
        var _Result = await this.ApiAccess.SendRequestAsync<SetActivityCompletionWebAppRequest, bool>(
            new SetActivityCompletionWebAppRequest() { IsComplete = true },
            ApiProvider.SetActivityCompletion(item.ID),
            _ => this.m_LoadFailed = true,
            this.m_CancellationTokenHandler.Token);

        if (_Result != true)
            return;

        await this.LoadCalendarAsync();
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.Activities, this.m_CancellationTokenHandler.Token);
    }

    #endregion Methods

}
