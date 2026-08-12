using Home.Domain.Entities;

namespace Home.Application.UseCases.LightScenes.GetLightScenes;

public interface IGetLightScenesOutputPort
{

    #region Methods

    Task PresentLightScenesAsync(IReadOnlyList<LightScene> scenes, CancellationToken cancellationToken);

    #endregion Methods

}
