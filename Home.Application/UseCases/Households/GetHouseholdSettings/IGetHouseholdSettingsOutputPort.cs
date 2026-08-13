using Home.Domain.Entities;

namespace Home.Application.UseCases.Households.GetHouseholdSettings;

public interface IGetHouseholdSettingsOutputPort
{

    #region Methods

    Task PresentHouseholdSettingsAsync(Household household, CancellationToken cancellationToken);

    #endregion Methods

}
