namespace Home.Application.UseCases.Activities.SetActivityCompletion;

public interface ISetActivityCompletionOutputPort
{

    #region Methods

    Task PresentActivityCompletionSetAsync(CancellationToken cancellationToken);
    Task PresentActivityNotFoundAsync(long activityID, CancellationToken cancellationToken);

    #endregion Methods

}
