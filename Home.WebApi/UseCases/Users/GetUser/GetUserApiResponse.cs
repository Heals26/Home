namespace Home.WebApi.UseCases.Users.GetUser;

/// <summary>
/// One member, as anyone in the household is allowed to see them.
/// <para>
/// This exists because the presenter used to return the <c>User</c> entity whole, which put the
/// stored password on the wire for every member who asked. A response model is the only way to
/// keep that from coming back the next time a column is added to the table.
/// </para>
/// </summary>
public class GetUserApiResponse
{

    #region Properties

    public string Email { get; set; }
    public string FirstName { get; set; }
    public string FullName { get; set; }
    public string LastName { get; set; }
    public string MiddleNames { get; set; }
    public long UserID { get; set; }

    #endregion Properties

}
