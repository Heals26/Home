using Home.Domain.Entities;

namespace Home.Application.Services.EntityLogic.Lights;

public interface ILightSyncLogic
{

    #region Methods

    /// <summary>
    /// Pulls the provider's bulb list and reconciles it into the household's records. Null when
    /// the provider is unreachable. The caller owns saving.
    /// </summary>
    Task<LightSyncResult?> SyncHouseholdAsync(Household household, CancellationToken cancellationToken);

    #endregion Methods

}
