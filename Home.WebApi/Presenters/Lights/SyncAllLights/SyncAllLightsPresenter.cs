using AutoMapper;
using Home.Application.UseCases.Lights.SyncAllLights;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.Lights.SyncAllLights;

/// <summary>
/// Not reached over HTTP — the background sync runner reads the counts off it directly, so this
/// presents into properties rather than an <c>IActionResult</c>.
/// </summary>
public class SyncAllLightsPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ISyncAllLightsOutputPort
{

    #region Properties

    public IReadOnlyList<long> SyncedHouseholdIDs { get; private set; } = [];

    public int UnavailableHouseholds { get; private set; }

    #endregion Properties

    #region Methods

    Task ISyncAllLightsOutputPort.PresentAllLightsSyncedAsync(IReadOnlyList<long> syncedHouseholdIDs, int unavailableHouseholds, CancellationToken cancellationToken)
    {
        this.SyncedHouseholdIDs = syncedHouseholdIDs;
        this.UnavailableHouseholds = unavailableHouseholds;

        return Task.CompletedTask;
    }

    #endregion Methods

}
