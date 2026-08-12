using AutoMapper;
using Home.Application.UseCases.LightGroups.AssignLightToGroup;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.LightGroups.AssignLightToGroup;

public class AssignLightToGroupPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IAssignLightToGroupOutputPort
{

    #region Methods

    Task IAssignLightToGroupOutputPort.PresentLightAssignedAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IAssignLightToGroupOutputPort.PresentLightGroupNotFoundAsync(long lightGroupID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Light Group {lightGroupID} Not Found", cancellationToken);

    Task IAssignLightToGroupOutputPort.PresentLightNotFoundAsync(string lightID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Light {lightID} Not Found", cancellationToken);

    #endregion Methods

}
