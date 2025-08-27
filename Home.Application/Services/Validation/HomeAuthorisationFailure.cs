namespace Home.Application.Services.Validation;
public class HomeAuthorisationFailure(string failure)
{

    #region Properties

    public string Failure { get; } = failure;

    #endregion Properties

}
