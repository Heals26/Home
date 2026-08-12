using CleanArchitecture.Mediator;
using Home.Application.UseCases.LightSchedules.RunDueLightSchedules;
using Home.WebApi.Presenters.LightSchedules.RunDueLightSchedules;

namespace Home.WebApi.Infrastructure.Lights;

/// <summary>
/// Ticks once a minute and fires any schedule that has come due.
/// <para>
/// This only works while the API process is alive. A tablet that sleeps will not run schedules —
/// the API has to live somewhere always-on for them to be dependable.
/// </para>
/// </summary>
internal class LightScheduleRunner(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    ILogger<LightScheduleRunner> logger)
    : BackgroundService
{

    #region Fields

    private static readonly TimeSpan s_Interval = TimeSpan.FromMinutes(1);

    #endregion Fields

    #region Methods

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Light schedule runner started; checking every {Interval}.", s_Interval);

        using var _Timer = new PeriodicTimer(s_Interval, timeProvider);

        while (await _Timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await this.RunOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception _Exception)
            {
                // A bad tick must never kill the runner, or schedules stop silently until restart.
                logger.LogError(_Exception, "Light schedule tick failed.");
            }
        }
    }

    /// <summary>
    /// The pipeline and its DbContext are scoped, so each tick gets its own scope rather than
    /// holding one for the life of the process.
    /// </summary>
    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        using var _Scope = scopeFactory.CreateScope();

        var _Pipeline = _Scope.ServiceProvider.GetRequiredService<Pipeline>();
        var _ServiceFactory = _Scope.ServiceProvider.GetRequiredService<ServiceFactory>();
        var _Presenter = _Scope.ServiceProvider.GetRequiredService<RunDueLightSchedulesPresenter>();

        await _Pipeline.InvokeAsync(
            new RunDueLightSchedulesInputPort(), _Presenter, _ServiceFactory, cancellationToken);

        if (_Presenter.Fired > 0 || _Presenter.Failed > 0)
        {
            logger.LogInformation("Light schedules: {Fired} fired, {Failed} could not reach the provider.",
                _Presenter.Fired, _Presenter.Failed);
        }
    }

    #endregion Methods

}
