using Home.WebApi.UseCases.Users.Models;

namespace Home.WebApi.UseCases.Users.GetUsers;

public class GetUsersApiResponse
{

    #region Properties

    public ICollection<UserSummaryDto> Users { get; set; } = [];

    #endregion Properties

}
