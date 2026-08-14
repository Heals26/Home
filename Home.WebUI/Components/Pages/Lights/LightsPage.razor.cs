using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.Components.Shared.Inputs;
using Home.WebUI.DataAccess.LightGroups.AssignLightToGroup;
using Home.WebUI.DataAccess.LightGroups.CreateLightGroup;
using Home.WebUI.DataAccess.LightGroups.SetLightGroupState;
using Home.WebUI.DataAccess.LightGroups.UpdateLightGroup;
using Home.WebUI.DataAccess.Lights.GetLights;
using Home.WebUI.DataAccess.Lights.Models;
using Home.WebUI.DataAccess.Lights.SetLightState;
using Home.WebUI.DataAccess.Lights.StartLightEffect;
using Home.WebUI.DataAccess.Lights.SyncLights;
using Home.WebUI.DataAccess.LightScenes.CaptureLightScene;
using Home.WebUI.DataAccess.LightScenes.GetLightScenes;
using Home.WebUI.DataAccess.LightScenes.Models;
using Home.WebUI.DataAccess.LightSchedules.CreateLightSchedule;
using Home.WebUI.DataAccess.LightSchedules.GetLightSchedules;
using Home.WebUI.DataAccess.LightSchedules.Models;
using Home.WebUI.DataAccess.LightSchedules.UpdateLightSchedule;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.CancellationTokens;
using Home.WebUI.Infrastructure.Services.ChangeNotifications;
using Home.WebUI.Infrastructure.Values;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Lights;

public partial class LightsPage : IDisposable
{

    #region Records

    private sealed record ScheduleDay(string Label, int Bit);

    #endregion Records

    #region Fields

    private CancellationTokenHandler m_CancellationTokenHandler = new();
    private ErrorHandler? m_ErrorHandler;
    private IDisposable? m_ChangeSubscription;
    private List<LightGroupDto>? m_Groups;
    private List<LightSceneDto>? m_Scenes;
    private List<LightScheduleDto>? m_Schedules;
    private string? m_LastSynced;
    private string m_NewGroupName = string.Empty;
    private bool m_EditMode;
    private bool m_Syncing;

    // Scenes
    private long? m_ApplyingSceneID;
    private bool m_ShowCaptureModal;
    private string m_CaptureName = string.Empty;
    private long m_CaptureGroupID;
    private bool m_Capturing;

    // Schedules
    private bool m_ShowScheduleModal;
    private string m_ScheduleName = string.Empty;
    private long m_ScheduleSceneID;
    private LightScheduleTrigger m_ScheduleTrigger = LightScheduleTrigger.Time;
    private string m_ScheduleTime = "19:00";
    private int m_ScheduleOffsetMinutes;
    private int m_ScheduleDays;
    private bool m_CreatingSchedule;

    private static readonly List<HomeSegmentedControl<LightScheduleTrigger>.SegmentOption> TriggerOptions =
    [
        new("At a time", LightScheduleTrigger.Time),
        new("Sunrise", LightScheduleTrigger.Sunrise),
        new("Sunset", LightScheduleTrigger.Sunset),
    ];

    private static readonly (string Label, int Minutes)[] OffsetOptions =
    [
        ("1 hour before", -60),
        ("30 min before", -30),
        ("15 min before", -15),
        ("Right on it", 0),
        ("15 min after", 15),
        ("30 min after", 30),
        ("1 hour after", 60),
    ];

    // Effects
    private bool m_ShowEffectsModal;
    private LightGroupDto? m_EffectsGroup;
    private LightEffectKind m_EffectKind = LightEffectKind.Breathe;
    private ColourPreset? m_EffectPreset;
    private bool m_StartingEffect;
    private bool m_StoppingEffect;

    // Bit 0 is Sunday, matching System.DayOfWeek, displayed Monday-first.
    private static readonly ScheduleDay[] ScheduleDays =
    [
        new("Mon", 1 << 1),
        new("Tue", 1 << 2),
        new("Wed", 1 << 3),
        new("Thu", 1 << 4),
        new("Fri", 1 << 5),
        new("Sat", 1 << 6),
        new("Sun", 1 << 0),
    ];

    private static readonly List<HomeSegmentedControl<LightEffectKind>.SegmentOption> EffectOptions =
    [
        new("Breathe", LightEffectKind.Breathe),
        new("Pulse", LightEffectKind.Pulse),
    ];

    #endregion Fields

    #region Properties

    [CascadingParameter(Name = "CancellationToken")] public CancellationToken CancellationToken { get; set; }

    #endregion Properties

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(
            this.LoadLightsAsync(),
            this.LoadScenesAsync(),
            this.LoadSchedulesAsync());

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

    private async Task OnHouseholdChangedAsync(ChangeArea area)
    {
        if (area != ChangeArea.Lights)
            return;

        await this.InvokeAsync(async () =>
        {
            await Task.WhenAll(this.LoadLightsAsync(), this.LoadScenesAsync(), this.LoadSchedulesAsync());
            this.StateHasChanged();
        });
    }

    private async Task PublishLightsChangedAsync()
        => await this.ChangeBroadcaster.PublishAsync(ChangeArea.Lights, this.m_CancellationTokenHandler.Token);

    private void ToggleEditMode()
        => this.m_EditMode = !this.m_EditMode;

    // Free — this reads Home's own records, not the provider.
    private async Task LoadLightsAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetLightsWebAppResponse>(
            null!, ApiProvider.GetLights(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != null)
            this.m_Groups = _Result.Groups;
    }

    private async Task LoadScenesAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetLightScenesWebAppResponse>(
            null!, ApiProvider.GetLightScenes(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != null)
            this.m_Scenes = _Result.Scenes;
    }

    private async Task LoadSchedulesAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetLightSchedulesWebAppResponse>(
            null!, ApiProvider.GetLightSchedules(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != null)
            this.m_Schedules = _Result.Schedules;
    }

    private async Task SyncLightsAsync()
    {
        if (this.m_Syncing)
            return;

        this.m_Syncing = true;

        var _Result = await this.ApiAccess.SendRequestAsync<object, SyncLightsWebAppResponse>(
            null!, ApiProvider.SyncLights(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_Syncing = false;

        if (_Result == null)
            return;

        this.m_LastSynced = this.TimeProvider.GetLocalNow().ToString("h:mm tt");

        // A sync can add or drop bulbs, which also changes what scenes cover.
        await Task.WhenAll(this.LoadLightsAsync(), this.LoadScenesAsync());
        await this.PublishLightsChangedAsync();
    }

    /* ---------- scenes ---------- */

    private async Task ApplySceneAsync(LightSceneDto scene)
    {
        if (this.m_ApplyingSceneID != null)
            return;

        this.m_ApplyingSceneID = scene.LightSceneID;

        var _Result = await this.ApiAccess.SendRequestAsync<object, bool>(
            null!, ApiProvider.ApplyLightScene(scene.LightSceneID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_ApplyingSceneID = null;

        if (_Result != true)
            return;

        await this.LoadLightsAsync();
        await this.PublishLightsChangedAsync();
    }

    private void OpenCaptureModal()
    {
        this.m_CaptureName = string.Empty;
        this.m_CaptureGroupID = 0;
        this.m_ShowCaptureModal = true;
    }

    private async Task CaptureSceneAsync()
    {
        if (this.m_Capturing)
            return;

        this.m_Capturing = true;

        var _Result = await this.ApiAccess.SendRequestAsync<CaptureLightSceneWebAppRequest, CaptureLightSceneWebAppResponse>(
            new()
            {
                Name = this.m_CaptureName.Trim(),
                LightGroupID = this.m_CaptureGroupID == 0 ? null : this.m_CaptureGroupID
            },
            ApiProvider.CaptureLightScene(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_Capturing = false;

        if (_Result == null)
            return;

        this.m_ShowCaptureModal = false;

        await this.LoadScenesAsync();
        await this.PublishLightsChangedAsync();
    }

    // Deleting a scene also deletes any schedule that fires it, so both lists reload.
    private async Task DeleteSceneAsync(LightSceneDto scene)
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, bool>(
            null!, ApiProvider.DeleteLightScene(scene.LightSceneID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != true)
            return;

        await Task.WhenAll(this.LoadScenesAsync(), this.LoadSchedulesAsync());
        await this.PublishLightsChangedAsync();
    }

    /* ---------- schedules ---------- */

    private void OpenScheduleModal()
    {
        this.m_ScheduleName = string.Empty;
        this.m_ScheduleSceneID = 0;
        this.m_ScheduleTrigger = LightScheduleTrigger.Time;
        this.m_ScheduleTime = "19:00";
        this.m_ScheduleOffsetMinutes = 0;
        this.m_ScheduleDays = 0;
        this.m_ShowScheduleModal = true;
    }

    private bool IsDaySelected(int bit)
        => (this.m_ScheduleDays & bit) != 0;

    private void ToggleDay(int bit)
        => this.m_ScheduleDays ^= bit;

    private bool CanCreateSchedule()
        => !string.IsNullOrWhiteSpace(this.m_ScheduleName)
            && this.m_ScheduleSceneID != 0
            && this.m_ScheduleDays != 0
            && (this.m_ScheduleTrigger != LightScheduleTrigger.Time || TimeSpan.TryParse(this.m_ScheduleTime, out _));

    private async Task CreateScheduleAsync()
    {
        if (this.m_CreatingSchedule || !this.CanCreateSchedule())
            return;

        this.m_CreatingSchedule = true;

        var _Result = await this.ApiAccess.SendRequestAsync<CreateLightScheduleWebAppRequest, CreateLightScheduleWebAppResponse>(
            new()
            {
                Name = this.m_ScheduleName.Trim(),
                LightSceneID = this.m_ScheduleSceneID,
                Trigger = this.m_ScheduleTrigger,
                TimeOfDay = this.m_ScheduleTrigger == LightScheduleTrigger.Time
                    ? TimeSpan.Parse(this.m_ScheduleTime)
                    : TimeSpan.Zero,
                OffsetMinutes = this.m_ScheduleTrigger == LightScheduleTrigger.Time ? 0 : this.m_ScheduleOffsetMinutes,
                DaysOfWeek = this.m_ScheduleDays
            },
            ApiProvider.CreateLightSchedule(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_CreatingSchedule = false;

        if (_Result == null)
            return;

        this.m_ShowScheduleModal = false;

        await this.LoadSchedulesAsync();
        await this.PublishLightsChangedAsync();
    }

    // The UI flips first so the toggle feels instant; a failed call reloads the truth.
    private async Task ToggleScheduleAsync(LightScheduleDto schedule, bool isEnabled)
    {
        schedule.IsEnabled = isEnabled;

        var _Result = await this.ApiAccess.SendRequestAsync<UpdateLightScheduleWebAppRequest, bool>(
            new() { IsEnabled = new(isEnabled) },
            ApiProvider.UpdateLightSchedule(schedule.LightScheduleID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != true)
        {
            await this.LoadSchedulesAsync();
            return;
        }

        await this.PublishLightsChangedAsync();
    }

    private async Task DeleteScheduleAsync(LightScheduleDto schedule)
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, bool>(
            null!, ApiProvider.DeleteLightSchedule(schedule.LightScheduleID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != true)
            return;

        await this.LoadSchedulesAsync();
        await this.PublishLightsChangedAsync();
    }

    private static string FormatTime(TimeSpan timeOfDay)
        => DateTime.MinValue.Add(timeOfDay).ToString("h:mm tt");

    private static string FormatTrigger(LightScheduleDto schedule)
    {
        if (schedule.Trigger == LightScheduleTrigger.Time)
            return FormatTime(schedule.TimeOfDay);

        var _Event = schedule.Trigger == LightScheduleTrigger.Sunrise ? "sunrise" : "sunset";

        return schedule.OffsetMinutes switch
        {
            0 => $"At {_Event}",
            < 0 => $"{Math.Abs(schedule.OffsetMinutes)} min before {_Event}",
            > 0 => $"{schedule.OffsetMinutes} min after {_Event}"
        };
    }

    private static string FormatDays(int daysOfWeek)
    {
        const int _EveryDay = 127;
        const int _Weekdays = 62;
        const int _Weekend = 65;

        return daysOfWeek switch
        {
            _EveryDay => "Every day",
            _Weekdays => "Weekdays",
            _Weekend => "Weekends",
            _ => string.Join(" · ", ScheduleDays.Where(d => (daysOfWeek & d.Bit) != 0).Select(d => d.Label))
        };
    }

    /* ---------- effects ---------- */

    private void OpenEffectsModal(LightGroupDto group)
    {
        this.m_EffectsGroup = group;
        this.m_EffectKind = LightEffectKind.Breathe;
        this.m_EffectPreset = ColourPreset.All.FirstOrDefault(p => !p.IsWhite);
        this.m_ShowEffectsModal = true;
    }

    private async Task StartEffectAsync()
    {
        if (this.m_StartingEffect || this.m_EffectsGroup == null || this.m_EffectPreset == null)
            return;

        this.m_StartingEffect = true;

        _ = await this.ApiAccess.SendRequestAsync<StartLightEffectWebAppRequest, bool>(
            new()
            {
                LightGroupID = this.m_EffectsGroup.LightGroupID,
                Kind = this.m_EffectKind,
                Hue = this.m_EffectPreset.Hue,
                Saturation = this.m_EffectPreset.Saturation,
                PeriodSeconds = 2,
                Cycles = 8
            },
            ApiProvider.StartLightEffect(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_StartingEffect = false;
    }

    private async Task StopEffectAsync()
    {
        if (this.m_StoppingEffect || this.m_EffectsGroup == null)
            return;

        this.m_StoppingEffect = true;

        _ = await this.ApiAccess.SendRequestAsync<StartLightEffectWebAppRequest, bool>(
            new()
            {
                LightGroupID = this.m_EffectsGroup.LightGroupID,
                Kind = LightEffectKind.Off
            },
            ApiProvider.StartLightEffect(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_StoppingEffect = false;
    }

    /* ---------- single light ---------- */

    private Task SetPowerAsync(LightDto light, bool isOn)
    {
        light.IsOn = isOn;
        return this.SendLightAsync(light, new() { IsOn = new(isOn) });
    }

    private Task SetBrightnessAsync(LightDto light, double brightness)
    {
        light.Brightness = brightness;
        return this.SendLightAsync(light, new() { Brightness = new(brightness) });
    }

    private Task ApplyPresetAsync(LightDto light, ColourPreset preset)
    {
        light.Hue = preset.Hue;
        light.Saturation = preset.Saturation;
        light.Kelvin = preset.Kelvin;

        var _Request = preset.IsWhite
            ? new SetLightStateWebAppRequest() { Kelvin = new(preset.Kelvin) }
            : new SetLightStateWebAppRequest() { Hue = new(preset.Hue), Saturation = new(preset.Saturation) };

        return this.SendLightAsync(light, _Request);
    }

    // The UI updates before the call so the tablet feels instant. On failure the list is reloaded,
    // which snaps the control back to whatever Home last knew.
    private async Task SendLightAsync(LightDto light, SetLightStateWebAppRequest request)
    {
        var _Result = await this.ApiAccess.SendRequestAsync<SetLightStateWebAppRequest, bool>(
            request, ApiProvider.SetLightState(light.ID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != true)
        {
            await this.LoadLightsAsync();
            return;
        }

        await this.PublishLightsChangedAsync();
    }

    /* ---------- whole group ---------- */

    // One request for the whole room rather than one per bulb.
    private async Task SetGroupPowerAsync(LightGroupDto group, bool isOn)
    {
        foreach (var _Light in group.Lights.Where(l => l.IsConnected))
            _Light.IsOn = isOn;

        var _Result = await this.ApiAccess.SendRequestAsync<SetLightGroupStateWebAppRequest, bool>(
            new() { IsOn = new(isOn) }, ApiProvider.SetLightGroupState(group.LightGroupID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != true)
        {
            await this.LoadLightsAsync();
            return;
        }

        await this.PublishLightsChangedAsync();
    }

    /* ---------- editing ---------- */

    private async Task CreateGroupAsync()
    {
        var _Name = this.m_NewGroupName.Trim();

        if (string.IsNullOrWhiteSpace(_Name))
            return;

        var _Result = await this.ApiAccess.SendRequestAsync<CreateLightGroupWebAppRequest, CreateLightGroupWebAppResponse>(
            new() { Name = _Name }, ApiProvider.CreateLightGroup(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result == null)
            return;

        this.m_NewGroupName = string.Empty;

        await this.LoadLightsAsync();
        await this.PublishLightsChangedAsync();
    }

    private async Task RenameGroupAsync(LightGroupDto group, string name)
    {
        group.Name = name;

        _ = await this.ApiAccess.SendRequestAsync<UpdateLightGroupWebAppRequest, bool>(
            new() { Name = new(name) }, ApiProvider.UpdateLightGroup(group.LightGroupID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        await this.PublishLightsChangedAsync();
    }

    // Swaps sequence numbers with the neighbour, then persists both.
    private async Task MoveGroupAsync(LightGroupDto group, int delta)
    {
        if (this.m_Groups == null)
            return;

        var _Index = this.m_Groups.IndexOf(group);
        var _Target = _Index + delta;

        if (_Index < 0 || _Target < 0 || _Target >= this.m_Groups.Count)
            return;

        var _Neighbour = this.m_Groups[_Target];

        (group.Sequence, _Neighbour.Sequence) = (_Neighbour.Sequence, group.Sequence);

        this.m_Groups[_Index] = _Neighbour;
        this.m_Groups[_Target] = group;

        foreach (var _Moved in new[] { group, _Neighbour })
        {
            _ = await this.ApiAccess.SendRequestAsync<UpdateLightGroupWebAppRequest, bool>(
                new() { Sequence = new(_Moved.Sequence) }, ApiProvider.UpdateLightGroup(_Moved.LightGroupID),
                e => this.m_ErrorHandler?.AddError(e),
                this.m_CancellationTokenHandler.Token);
        }

        await this.PublishLightsChangedAsync();
    }

    private async Task DeleteGroupAsync(LightGroupDto group)
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, bool>(
            null!, ApiProvider.DeleteLightGroup(group.LightGroupID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != true)
            return;

        await this.LoadLightsAsync();
        await this.PublishLightsChangedAsync();
    }

    private async Task MoveLightAsync(LightDto light, long lightGroupID)
    {
        var _Result = await this.ApiAccess.SendRequestAsync<AssignLightToGroupWebAppRequest, bool>(
            new() { LightID = light.ID }, ApiProvider.AssignLightToGroup(lightGroupID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != true)
            return;

        await this.LoadLightsAsync();
        await this.PublishLightsChangedAsync();
    }

    #endregion Methods

}
