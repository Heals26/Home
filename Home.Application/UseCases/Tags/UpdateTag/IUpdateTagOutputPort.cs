using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.Tags.UpdateTag;

public interface IUpdateTagOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task<ContinuationBehaviour> PresentTagNameTakenAsync(string name, CancellationToken cancellationToken);
    Task PresentTagNotFoundAsync(long tagID, CancellationToken cancellationToken);
    Task PresentTagUpdatedAsync(CancellationToken cancellationToken);

    #endregion Methods

}
