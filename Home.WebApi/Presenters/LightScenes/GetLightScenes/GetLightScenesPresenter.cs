using AutoMapper;
using Home.Application.UseCases.LightScenes.GetLightScenes;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.LightScenes.GetLightScenes;
using Home.WebApi.UseCases.LightScenes.Models;

namespace Home.WebApi.Presenters.LightScenes.GetLightScenes;

public class GetLightScenesPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetLightScenesOutputPort
{

    #region Methods

    Task IGetLightScenesOutputPort.PresentLightScenesAsync(IReadOnlyList<LightScene> scenes, CancellationToken cancellationToken)
        => this.OkAsync(new GetLightScenesApiResponse()
        {
            Scenes = [.. scenes.Select(s => new LightSceneDto()
            {
                LightSceneID = s.LightSceneID,
                Name = s.Name,
                Sequence = s.Sequence,
                LightCount = s.States.Count
            })]
        }, cancellationToken);

    #endregion Methods

}
