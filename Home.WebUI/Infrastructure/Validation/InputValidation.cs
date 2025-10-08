using System.ComponentModel.DataAnnotations;

namespace Home.WebUI.Infrastructure.Validation;

public static class InputValidation
{

    #region Methods

    public static EmailAddressAttribute EmailValidator()
        => new() { ErrorMessage = "The email address is invalid." };

    #endregion Methods

}
