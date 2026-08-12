using AutoMapper;
using Home.Application.Services.Lights;
using Home.Application.UseCases.Lights.GetLights;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Lights.GetLights;
using Home.WebApi.UseCases.Lights.Models;

namespace Home.WebApi.Presenters.Lights.GetLights;

public class GetLightsPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetLightsOutputPort
{

    #region Methods

    Task IGetLightsOutputPort.PresentLightsAsync(IReadOnlyList<LightSnapshot> lights, CancellationToken cancellationToken)
        => this.OkAsync(new GetLightsApiResponse()
        {
            Lights = [.. lights
                .OrderBy(l => l.GroupName)
                .ThenBy(l => l.Label)
                .Select(l => new LightDto()
                {
                    ID = l.ID,
                    Label = l.Label,
                    GroupName = l.GroupName,
                    IsConnected = l.IsConnected,
                    IsOn = l.IsOn,
                    Brightness = l.Brightness,
                    Hue = l.Hue,
                    Saturation = l.Saturation,
                    Kelvin = l.Kelvin
                })]
        }, cancellationToken);

    Task IGetLightsOutputPort.PresentLightsUnavailableAsync(CancellationToken cancellationToken)
        => this.ServiceUnavailableAsync("The lighting service could not be reached", cancellationToken);

    #endregion Methods

}
