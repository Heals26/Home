namespace Home.Application.UseCases.Users.DeleteUser;

public interface IDeleteUserOutputPort
{

    #region Methods

    Task PresentUserDeletedNoContentAsync(CancellationToken cancellationToken);

    #endregion Methods

}
