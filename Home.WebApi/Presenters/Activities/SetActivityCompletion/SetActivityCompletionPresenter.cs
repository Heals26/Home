using AutoMapper;
using Home.Application.UseCases.Activities.SetActivityCompletion;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.Activities.SetActivityCompletion;

public class SetActivityCompletionPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ISetActivityCompletionOutputPort
{

    #region Methods

    Task ISetActivityCompletionOutputPort.PresentActivityCompletionSetAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task ISetActivityCompletionOutputPort.PresentActivityNotFoundAsync(long activityID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Activity {activityID} Not Found", cancellationToken);

    #endregion Methods

}
