using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.Tags.CreateTag;

public interface ICreateTagOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task PresentTagCreatedAsync(long tagID, CancellationToken cancellationToken);
    Task<ContinuationBehaviour> PresentTagNameTakenAsync(string name, CancellationToken cancellationToken);

    #endregion Methods

}
