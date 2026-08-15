namespace Home.Application.UseCases.Tags.DeleteTag;

public interface IDeleteTagOutputPort
{

    #region Methods

    Task PresentTagDeletedAsync(CancellationToken cancellationToken);
    Task PresentTagNotFoundAsync(long tagID, CancellationToken cancellationToken);

    #endregion Methods

}
