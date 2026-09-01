using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.CardSections.CreateCardSection;

public interface ICreateCardSectionOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task PresentCardSectionCreatedAsync(long cardSectionID, CancellationToken cancellationToken);

    #endregion Methods

}
