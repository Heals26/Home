using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.Support.CreateFeedback;

public interface ICreateFeedbackOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task PresentFeedbackCreatedAsync(long feedbackID, CancellationToken cancellationToken);

    #endregion Methods

}
