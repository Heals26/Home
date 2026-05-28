namespace Home.Application.UseCases.ActivityContents.UpdateActivityContent;

public interface IUpdateActivityContentOutputPort
{

    #region Methods

    Task PresentActivityContentNoContentAsync(CancellationToken cancellationToken);
    Task PresentActivityContentNotFoundAsync(long activityContentID, CancellationToken cancellationToken);

    #endregion Methods

}
