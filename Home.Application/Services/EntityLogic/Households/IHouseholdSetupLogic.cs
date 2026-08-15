using Home.Domain.Entities;

namespace Home.Application.Services.EntityLogic.Households;

public interface IHouseholdSetupLogic
{

    #region Methods

    /// <summary>
    /// Gives a new household the board columns and meal slots it needs to be usable on the first
    /// screen it ever shows. The caller owns saving.
    /// </summary>
    void SeedDefaults(Household household);

    #endregion Methods

}
