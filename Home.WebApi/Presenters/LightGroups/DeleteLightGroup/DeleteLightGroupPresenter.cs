using AutoMapper;
using Home.Application.UseCases.LightGroups.DeleteLightGroup;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.LightGroups.DeleteLightGroup;

public class DeleteLightGroupPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IDeleteLightGroupOutputPort
{

    #region Methods

    Task IDeleteLightGroupOutputPort.PresentLightGroupDeletedAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IDeleteLightGroupOutputPort.PresentLightGroupNotEmptyAsync(long lightGroupID, int lightCount, CancellationToken cancellationToken)
        => this.UnprocessableContent(new()
        {
            Detail = $"Move its {lightCount} light(s) to another group first.",
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = "The group still has lights in it.",
            Type = "https://datatracker.ietf.org/doc/html/rfc4918#section-11.2"
        }, cancellationToken);

    Task IDeleteLightGroupOutputPort.PresentLightGroupNotFoundAsync(long lightGroupID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Light Group {lightGroupID} Not Found", cancellationToken);

    #endregion Methods

}
