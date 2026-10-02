using AutoMapper;
using Home.Application.UseCases.Support.CreateFeedback;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Support.CreateFeedback;

namespace Home.WebApi.Presenters.Support.CreateFeedback;

public class CreateFeedbackPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ICreateFeedbackOutputPort
{

    #region Methods

    Task ICreateFeedbackOutputPort.PresentFeedbackCreatedAsync(long feedbackID, CancellationToken cancellationToken)
        => this.CreatedAsync(feedbackID, new CreateFeedbackApiResponse() { FeedbackID = feedbackID }, cancellationToken);

    #endregion Methods

}
