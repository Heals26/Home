using CleanArchitecture.Mediator;
using Home.Application.UseCases.CalendarSubscriptions.RefreshAllCalendarSubscriptions;
using Home.WebApi.Infrastructure.ChangeNotifications;
using Home.WebApi.Presenters.CalendarSubscriptions.RefreshAllCalendarSubscriptions;
using Microsoft.AspNetCore.SignalR;

namespace Home.WebApi.Infrastructure.Calendar;

/// <summary>
/// Re-reads every household's subscribed calendars on a timer, so a lesson moved in Google
/// Calendar shows up on the kitchen wall without anyone doing anything. One fetch per feed per
/// tick however many screens are open; the tablets read Home's own rows.
/// </summary>
internal class CalendarSubscriptionRunner(
    IHubContext<ChangeNotificationsHub> changeHub,
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    ILogger<CalendarSubscriptionRunner> logger)
    : BackgroundService
{

    #region Methods

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Calendar subscription runner started; refreshing every {Interval}.", CalendarFeedValues.RefreshInterval);

        using var _Timer = new PeriodicTimer(CalendarFeedValues.RefreshInterval, timeProvider);

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
                // A bad tick must never kill the runner, or every subscribed calendar quietly stops.
                logger.LogError(_Exception, "Calendar subscription refresh tick failed.");
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
        var _Presenter = _Scope.ServiceProvider.GetRequiredService<RefreshAllCalendarSubscriptionsPresenter>();

        await _Pipeline.InvokeAsync(new RefreshAllCalendarSubscriptionsInputPort(), _Presenter, _ServiceFactory, cancellationToken);

        foreach (var _HouseholdID in _Presenter.RefreshedHouseholdIDs)
            await changeHub.Clients.Group(ChangeNotificationsHub.GroupName(_HouseholdID)).SendAsync("Changed", "Calendar", cancellationToken);

        if (_Presenter.UnreachableSubscriptions > 0)
        {
            logger.LogInformation("Calendar subscriptions: {Refreshed} households refreshed, {Unreachable} feeds could not be read.",
                _Presenter.RefreshedHouseholdIDs.Count, _Presenter.UnreachableSubscriptions);
        }
    }

    #endregion Methods

}
