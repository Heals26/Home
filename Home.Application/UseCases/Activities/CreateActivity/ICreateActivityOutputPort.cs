namespace Home.Application.UseCases.Activities.CreateActivity;

public interface ICreateActivityOutputPort
{

    #region Methods

    Task PresentActivityCreatedAsync(long activityID, CancellationToken cancellationToken);

    #endregion Methods

}
