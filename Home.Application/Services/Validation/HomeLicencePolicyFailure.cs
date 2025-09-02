namespace Home.Application.Services.Validation;

public class HomeLicencePolicyFailure(string failure)
{

    #region Properties

    public string Failure { get; } = failure;

    #endregion Properties

}
