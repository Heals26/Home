using Home.Application.Infrastructure.Values;
using Home.Application.Services.Undo;

namespace Home.WebApi.Infrastructure.Undo;

/// <summary>
/// Carries out the deletes held back for an undo, once no undo can reach them any more.
/// </summary>
internal class UndoPurgeRunner(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    ILogger<UndoPurgeRunner> logger)
    : BackgroundService
{

    #region Methods

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var _Timer = new PeriodicTimer(UndoValues.PurgeInterval, timeProvider);

        while (await _Timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var _Scope = scopeFactory.CreateScope();

                var _Failures = await _Scope.ServiceProvider.GetRequiredService<IUndoStore>()
                    .PurgeAsync(timeProvider.GetUtcNow().UtcDateTime - UndoValues.PurgedAfter, stoppingToken);

                if (_Failures > 0)
                    logger.LogWarning("Undo purge could not delete {Failures} kinds of held-back row, which stay hidden and are tried again.", _Failures);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception _Exception)
            {
                // A bad tick must never kill the runner, or held-back rows pile up for good.
                logger.LogError(_Exception, "Undo purge tick failed.");
            }
        }
    }

    #endregion Methods

}
