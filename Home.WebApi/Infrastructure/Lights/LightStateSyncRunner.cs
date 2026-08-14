using CleanArchitecture.Mediator;
using Home.Application.UseCases.Lights.SyncAllLights;
using Home.WebApi.Infrastructure.ChangeNotifications;
using Home.WebApi.Presenters.Lights.SyncAllLights;
using Microsoft.AspNetCore.SignalR;

namespace Home.WebApi.Infrastructure.Lights;

/// <summary>
/// Refreshes bulb state from the provider every few minutes, so the dashboard and Lights page
/// reflect a light someone switched at the wall without anyone pressing Sync. One poll per tick
/// regardless of how many screens are open — the tablets read Home's own records.
/// </summary>
internal class LightStateSyncRunner(
    IHubContext<ChangeNotificationsHub> changeHub,
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    ILogger<LightStateSyncRunner> logger)
    : BackgroundService
{

    #region Fields

    private static readonly TimeSpan s_Interval = TimeSpan.FromMinutes(5);

    #endregion Fields

    #region Methods

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Light state sync runner started; refreshing every {Interval}.", s_Interval);

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
                // A bad tick must never kill the runner, or the board quietly goes stale.
                logger.LogError(_Exception, "Light state sync tick failed.");
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
        var _Presenter = _Scope.ServiceProvider.GetRequiredService<SyncAllLightsPresenter>();

        await _Pipeline.InvokeAsync(new SyncAllLightsInputPort(), _Presenter, _ServiceFactory, cancellationToken);

        foreach (var _HouseholdID in _Presenter.SyncedHouseholdIDs)
            await changeHub.Clients.Group(ChangeNotificationsHub.GroupName(_HouseholdID)).SendAsync("Changed", "Lights", cancellationToken);

        if (_Presenter.UnavailableHouseholds > 0)
        {
            logger.LogInformation("Light state sync: {Synced} synced, {Unavailable} could not reach the provider.",
                _Presenter.SyncedHouseholdIDs.Count, _Presenter.UnavailableHouseholds);
        }
    }

    #endregion Methods

}
