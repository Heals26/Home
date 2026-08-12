using AutoMapper;
using Home.Application.UseCases.Lights.GetLights;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Lights.GetLights;
using Home.WebApi.UseCases.Lights.Models;

namespace Home.WebApi.Presenters.Lights.GetLights;

public class GetLightsPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetLightsOutputPort
{

    #region Methods

    Task IGetLightsOutputPort.PresentLightsAsync(IReadOnlyList<LightGroup> groups, CancellationToken cancellationToken)
        => this.OkAsync(new GetLightsApiResponse()
        {
            Groups = [.. groups.Select(g => new LightGroupDto()
            {
                LightGroupID = g.LightGroupID,
                Name = g.Name,
                Sequence = g.Sequence,
                Lights = [.. g.Lights.OrderBy(l => l.Name).Select(l => new LightDto()
                {
                    ID = l.ID,
                    Label = l.Name,
                    IsConnected = l.IsConnected,
                    IsOn = l.IsOn,
                    Brightness = l.Brightness,
                    Hue = l.Hue,
                    Saturation = l.Saturation,
                    Kelvin = l.Kelvin,
                    StateUpdatedUTC = l.StateUpdatedUTC
                })]
            })]
        }, cancellationToken);

    #endregion Methods

}
