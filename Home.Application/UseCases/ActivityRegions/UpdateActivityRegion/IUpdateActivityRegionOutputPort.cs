namespace Home.Application.UseCases.ActivityRegions.UpdateActivityRegion;

public interface IUpdateActivityRegionOutputPort
{

    #region Methods

    Task PresentActivityRegionNoContentAsync(CancellationToken cancellationToken);
    Task PresentActivityRegionNotFoundAsync(long activityRegionID, CancellationToken cancellationToken);

    #endregion Methods

}
