using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.Lights;
using Home.Application.Services.EntityLogic.Lights;
using Home.Application.Services.Lights;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;
using Home.Domain.Enumerations;

namespace Home.Application.UseCases.LightSchedules.RunDueLightSchedules;

/// <summary>
/// Runs every schedule that has come due, across every household. Deliberately does not use
/// <c>IAuthorisationService</c> — there is no signed-in user behind a timer tick.
/// </summary>
internal class RunDueLightSchedulesInteractor
    : IInteractor<RunDueLightSchedulesInputPort, IRunDueLightSchedulesOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        RunDueLightSchedulesInputPort inputPort,
        IRunDueLightSchedulesOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _SceneLogic = serviceFactory.GetService<ILightSceneLogic>();
        var _TimeProvider = serviceFactory.GetService<TimeProvider>();

        var _LocalNow = _TimeProvider.GetLocalNow();

        var _Schedules = _PersistenceContext.GetEntities<LightSchedule>()
            .Where(s => s.IsEnabled)
            .Select(s => new
            {
                Schedule = s,
                Scene = s.Scene,
                Household = s.Scene.Household,
                States = s.Scene.States.Select(st => new { State = st, st.Light })
            })
            .ToList()
            .Select(s => s.Schedule)
            .Where(s => IsDue(s, _LocalNow))
            .ToList();

        if (_Schedules.Count == 0)
        {
            await outputPort.PresentSchedulesRunAsync(0, 0, cancellationToken);
            return;
        }

        var _Fired = 0;
        var _Failed = 0;

        foreach (var _Schedule in _Schedules)
        {
            var _Result = await _SceneLogic.ApplyAsync(_Schedule.Scene, cancellationToken);

            if (_Result == LightCommandResult.Unavailable)
            {
                // Leave LastRunUTC alone so the next tick retries rather than skipping the day.
                _Failed++;
                continue;
            }

            _Schedule.LastRunUTC = _TimeProvider.GetUtcNow().UtcDateTime;
            _Fired++;
        }

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentSchedulesRunAsync(_Fired, _Failed, cancellationToken);
    }

    /// <summary>
    /// What time the schedule wants to fire today: its fixed time, or today's sun event plus the
    /// offset. Null means it cannot fire today — no household location, polar day or night, or an
    /// offset that pushed the event out of the day.
    /// </summary>
    private static TimeSpan? GetDueTime(LightSchedule schedule, DateTimeOffset localNow)
    {
        if (schedule.Trigger == LightScheduleTrigger.Time)
            return schedule.TimeOfDay;

        var _Household = schedule.Scene.Household;

        if (_Household.Latitude == null || _Household.Longitude == null)
            return null;

        var _Event = SunCalculator.GetSunEventLocalTime(
            DateOnly.FromDateTime(localNow.Date),
            _Household.Latitude.Value,
            _Household.Longitude.Value,
            localNow.Offset,
            schedule.Trigger == LightScheduleTrigger.Sunrise);

        if (_Event == null)
            return null;

        var _DueTime = _Event.Value + TimeSpan.FromMinutes(schedule.OffsetMinutes);

        return _DueTime >= TimeSpan.Zero && _DueTime < TimeSpan.FromDays(1) ? _DueTime : null;
    }

    /// <summary>
    /// Due when today is one of its days, the trigger time has passed, and it has not already run
    /// today. The window is open-ended rather than an exact minute match so a tick the runner
    /// missed — machine asleep, long GC — still fires late rather than being skipped entirely.
    /// </summary>
    private static bool IsDue(LightSchedule schedule, DateTimeOffset localNow)
    {
        if ((schedule.DaysOfWeek & (1 << (int)localNow.DayOfWeek)) == 0)
            return false;

        var _DueTime = GetDueTime(schedule, localNow);

        if (_DueTime == null || localNow.TimeOfDay < _DueTime)
            return false;

        if (schedule.LastRunUTC == null)
            return true;

        // Compare in the same frame as localNow, whose offset the caller's clock decides.
        var _LastRunLocal = new DateTimeOffset(schedule.LastRunUTC.Value, TimeSpan.Zero)
            .ToOffset(localNow.Offset);

        return _LastRunLocal.Date < localNow.Date;
    }

    #endregion Methods

}
