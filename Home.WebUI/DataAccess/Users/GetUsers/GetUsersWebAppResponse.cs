using Home.WebUI.DataAccess.Users.Models;

namespace Home.WebUI.DataAccess.Users.GetUsers;

public class GetUsersWebAppResponse
{

    #region Properties

    /// <summary>
    /// The household's members, ordered by first name.
    /// </summary>
    public ICollection<UserSummaryDto> Users { get; set; } = [];

    #endregion Properties

}
