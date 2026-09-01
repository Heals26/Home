using AutoMapper;
using Home.Application.UseCases.LightScenes.SetLightSceneSequence;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.LightScenes.SetLightSceneSequence;

public class SetLightSceneSequencePresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ISetLightSceneSequenceOutputPort
{

    #region Methods

    Task ISetLightSceneSequenceOutputPort.PresentLightSceneNotFoundAsync(long lightSceneID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Light Scene {lightSceneID} Not Found", cancellationToken);

    Task ISetLightSceneSequenceOutputPort.PresentLightSceneSequenceSetAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    #endregion Methods

}
