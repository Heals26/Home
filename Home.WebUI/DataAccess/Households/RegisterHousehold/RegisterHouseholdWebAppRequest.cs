namespace Home.WebUI.DataAccess.Households.RegisterHousehold;

public class RegisterHouseholdWebAppRequest
{

    #region Properties

    /// <summary>
    /// The first member's sign-in email.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The first member's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// The household's display name, e.g. "The Healys".
    /// </summary>
    public string HouseholdName { get; set; } = string.Empty;

    /// <summary>
    /// The first member's last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// The first member's password.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    #endregion Properties

}
