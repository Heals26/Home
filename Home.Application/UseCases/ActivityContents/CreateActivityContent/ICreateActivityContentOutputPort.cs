namespace Home.Application.UseCases.ActivityContents.CreateActivityContent;

public interface ICreateActivityContentOutputPort
{

    #region Methods

    Task PresentActivityContentCreatedAsync(long activityContentID, CancellationToken cancellationToken);
    Task PresentActivityRegionNotFoundAsync(long activityRegionID, CancellationToken cancellationToken);

    #endregion Methods

}
