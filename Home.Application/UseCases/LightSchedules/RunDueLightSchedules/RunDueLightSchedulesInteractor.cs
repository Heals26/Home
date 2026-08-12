using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.Lights;
using Home.Application.Services.Lights;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

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
    /// Due when today is one of its days, the time has passed, and it has not already run today.
    /// The window is open-ended rather than an exact minute match so a tick the runner missed —
    /// machine asleep, long GC — still fires late rather than being skipped entirely.
    /// </summary>
    private static bool IsDue(LightSchedule schedule, DateTimeOffset localNow)
    {
        if ((schedule.DaysOfWeek & (1 << (int)localNow.DayOfWeek)) == 0)
            return false;

        if (localNow.TimeOfDay < schedule.TimeOfDay)
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
