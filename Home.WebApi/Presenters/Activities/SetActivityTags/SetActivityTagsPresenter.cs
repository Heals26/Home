using AutoMapper;
using Home.Application.UseCases.Activities.SetActivityTags;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.Activities.SetActivityTags;

public class SetActivityTagsPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ISetActivityTagsOutputPort
{

    #region Methods

    Task ISetActivityTagsOutputPort.PresentActivityNotFoundAsync(long activityID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Activity {activityID} Not Found", cancellationToken);

    Task ISetActivityTagsOutputPort.PresentActivityTagsSetAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task ISetActivityTagsOutputPort.PresentTagNotFoundAsync(long tagID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Tag {tagID} Not Found", cancellationToken);

    #endregion Methods

}
