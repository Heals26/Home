namespace Home.Application.UseCases.ActivityRegions.CreateActivityRegion;

public interface ICreateActivityRegionOutputPort
{

    #region Methods

    Task PresentActivityRegionCreatedAsync(long activityRegionID, CancellationToken cancellationToken);
    Task PresentActivityNotFoundAsync(long activityID, CancellationToken cancellationToken);

    #endregion Methods

}
