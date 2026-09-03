using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.ActivityRegions.CreateActivityRegion;

public interface ICreateActivityRegionOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task PresentActivityNotFoundAsync(long activityID, CancellationToken cancellationToken);
    Task PresentActivityRegionCreatedAsync(long activityRegionID, CancellationToken cancellationToken);

    /// <summary>
    /// The section does not exist, or belongs to another household — which reads the same to a
    /// caller who is not entitled to know the difference.
    /// </summary>
    Task PresentCardSectionNotFoundAsync(long cardSectionID, CancellationToken cancellationToken);

    #endregion Methods

}
