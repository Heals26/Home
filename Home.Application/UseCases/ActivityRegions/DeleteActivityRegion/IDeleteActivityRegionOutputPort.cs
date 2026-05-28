namespace Home.Application.UseCases.ActivityRegions.DeleteActivityRegion;

public interface IDeleteActivityRegionOutputPort
{

    #region Methods

    Task PresentActivityRegionDeletedNoContentAsync(CancellationToken cancellationToken);
    Task PresentActivityRegionNotFoundAsync(long activityRegionID, CancellationToken cancellationToken);

    #endregion Methods

}
