using AutoMapper;
using Home.Application.UseCases.LightGroups.SetLightGroupState;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.LightGroups.SetLightGroupState;

public class SetLightGroupStatePresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ISetLightGroupStateOutputPort
{

    #region Methods

    Task ISetLightGroupStateOutputPort.PresentLightGroupNotFoundAsync(long lightGroupID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Light Group {lightGroupID} Not Found", cancellationToken);

    Task ISetLightGroupStateOutputPort.PresentLightGroupStateSetAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task ISetLightGroupStateOutputPort.PresentLightsUnavailableAsync(CancellationToken cancellationToken)
        => this.ServiceUnavailableAsync("The lighting service could not be reached", cancellationToken);

    Task ISetLightGroupStateOutputPort.PresentNothingToChangeAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    #endregion Methods

}
