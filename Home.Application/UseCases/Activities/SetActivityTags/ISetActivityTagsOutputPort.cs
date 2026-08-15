namespace Home.Application.UseCases.Activities.SetActivityTags;

public interface ISetActivityTagsOutputPort
{

    #region Methods

    Task PresentActivityNotFoundAsync(long activityID, CancellationToken cancellationToken);
    Task PresentActivityTagsSetAsync(CancellationToken cancellationToken);
    Task PresentTagNotFoundAsync(long tagID, CancellationToken cancellationToken);

    #endregion Methods

}
