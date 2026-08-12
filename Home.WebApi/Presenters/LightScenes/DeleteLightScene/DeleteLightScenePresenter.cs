using AutoMapper;
using Home.Application.UseCases.LightScenes.DeleteLightScene;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.LightScenes.DeleteLightScene;

public class DeleteLightScenePresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IDeleteLightSceneOutputPort
{

    #region Methods

    Task IDeleteLightSceneOutputPort.PresentLightSceneDeletedAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IDeleteLightSceneOutputPort.PresentLightSceneNotFoundAsync(long lightSceneID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Light Scene {lightSceneID} Not Found", cancellationToken);

    #endregion Methods

}
