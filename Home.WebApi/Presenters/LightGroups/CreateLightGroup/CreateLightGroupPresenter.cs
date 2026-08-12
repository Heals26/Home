using AutoMapper;
using Home.Application.UseCases.LightGroups.CreateLightGroup;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.LightGroups.CreateLightGroup;

namespace Home.WebApi.Presenters.LightGroups.CreateLightGroup;

public class CreateLightGroupPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ICreateLightGroupOutputPort
{

    #region Methods

    Task ICreateLightGroupOutputPort.PresentLightGroupCreatedAsync(long lightGroupID, CancellationToken cancellationToken)
        => this.CreatedAsync(lightGroupID, new CreateLightGroupApiResponse() { LightGroupID = lightGroupID }, cancellationToken);

    Task ICreateLightGroupOutputPort.PresentNoLocationAsync(CancellationToken cancellationToken)
        => this.NotFoundAsync("No lights have been synced yet, so there is nowhere to put a group", cancellationToken);

    #endregion Methods

}
