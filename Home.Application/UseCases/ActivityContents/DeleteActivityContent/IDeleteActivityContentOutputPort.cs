namespace Home.Application.UseCases.ActivityContents.DeleteActivityContent;

public interface IDeleteActivityContentOutputPort
{

    #region Methods

    Task PresentActivityContentDeletedNoContentAsync(CancellationToken cancellationToken);
    Task PresentActivityContentNotFoundAsync(long activityContentID, CancellationToken cancellationToken);

    #endregion Methods

}
