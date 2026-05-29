namespace Home.Application.UseCases.Activities.UpdateActivity;

public interface IUpdateActivityOutputPort
{

    #region Methods

    Task PresentActivityNoContentAsync(CancellationToken cancellationToken);
    Task PresentActivityNotFoundAsync(long activityID, CancellationToken cancellationToken);

    #endregion Methods

}
