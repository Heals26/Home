using AutoMapper;
using Home.Application.UseCases.Households.RegisterHousehold;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Households.RegisterHousehold;

namespace Home.WebApi.Presenters.Households.RegisterHousehold;

public class RegisterHouseholdPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IRegisterHouseholdOutputPort
{

    #region Methods

    Task IRegisterHouseholdOutputPort.PresentHouseholdRegisteredAsync(long householdID, CancellationToken cancellationToken)
        => this.CreatedAsync(householdID, new RegisterHouseholdApiResponse() { HouseholdID = householdID }, cancellationToken);

    Task IRegisterHouseholdOutputPort.PresentRegistrationClosedAsync(CancellationToken cancellationToken)
        => this.ConflictAsync(cancellationToken);

    #endregion Methods

}
