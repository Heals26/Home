using Home.Domain.Entities;

namespace Home.Application.UseCases.Lights.GetLights;

public interface IGetLightsOutputPort
{

    #region Methods

    Task PresentLightsAsync(IReadOnlyList<LightGroup> groups, CancellationToken cancellationToken);

    #endregion Methods

}
