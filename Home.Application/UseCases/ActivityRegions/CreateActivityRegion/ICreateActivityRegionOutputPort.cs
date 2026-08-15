using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.ActivityRegions.CreateActivityRegion;

public interface ICreateActivityRegionOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task PresentActivityNotFoundAsync(long activityID, CancellationToken cancellationToken);
    Task PresentActivityRegionCreatedAsync(long activityRegionID, CancellationToken cancellationToken);

    #endregion Methods

}
