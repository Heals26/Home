using AutoMapper;
using Home.Application.UseCases.Households.GetSetupStatus;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Households.GetSetupStatus;

namespace Home.WebApi.Presenters.Households.GetSetupStatus;

public class GetSetupStatusPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetSetupStatusOutputPort
{

    #region Methods

    Task IGetSetupStatusOutputPort.PresentSetupStatusAsync(bool requiresSetup, CancellationToken cancellationToken)
        => this.OkAsync(new GetSetupStatusApiResponse() { RequiresSetup = requiresSetup }, cancellationToken);

    #endregion Methods

}
