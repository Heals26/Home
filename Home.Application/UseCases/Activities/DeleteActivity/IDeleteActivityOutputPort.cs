namespace Home.Application.UseCases.Activities.DeleteActivity;

public interface IDeleteActivityOutputPort
{

    #region Methods

    Task PresentActivityDeletedNoContentAsync(CancellationToken cancellationToken);

    #endregion Methods

}
