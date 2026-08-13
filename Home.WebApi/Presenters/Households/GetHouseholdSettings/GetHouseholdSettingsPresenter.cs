using AutoMapper;
using Home.Application.UseCases.Households.GetHouseholdSettings;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Households.GetHouseholdSettings;

namespace Home.WebApi.Presenters.Households.GetHouseholdSettings;

public class GetHouseholdSettingsPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetHouseholdSettingsOutputPort
{

    #region Methods

    Task IGetHouseholdSettingsOutputPort.PresentHouseholdSettingsAsync(Household household, CancellationToken cancellationToken)
        => this.OkAsync(new GetHouseholdSettingsApiResponse()
        {
            HasLifxApiToken = !string.IsNullOrEmpty(household.LifxApiToken),
            Latitude = household.Latitude,
            Longitude = household.Longitude,
            Name = household.Name
        }, cancellationToken);

    #endregion Methods

}
