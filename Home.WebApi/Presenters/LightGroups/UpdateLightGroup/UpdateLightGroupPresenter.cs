using AutoMapper;
using Home.Application.UseCases.LightGroups.UpdateLightGroup;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.LightGroups.UpdateLightGroup;

public class UpdateLightGroupPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IUpdateLightGroupOutputPort
{

    #region Methods

    Task IUpdateLightGroupOutputPort.PresentLightGroupNotFoundAsync(long lightGroupID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Light Group {lightGroupID} Not Found", cancellationToken);

    Task IUpdateLightGroupOutputPort.PresentLightGroupUpdatedAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    #endregion Methods

}
