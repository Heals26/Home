using AutoMapper;
using Home.Application.UseCases.Households.UpdateHouseholdSettings;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.Households.UpdateHouseholdSettings;

public class UpdateHouseholdSettingsPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IUpdateHouseholdSettingsOutputPort
{

    #region Methods

    Task IUpdateHouseholdSettingsOutputPort.PresentHouseholdSettingsUpdatedAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    #endregion Methods

}
