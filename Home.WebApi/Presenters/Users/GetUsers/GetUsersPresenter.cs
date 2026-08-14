using AutoMapper;
using Home.Application.UseCases.Users.GetUsers;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Users.GetUsers;
using Home.WebApi.UseCases.Users.Models;

namespace Home.WebApi.Presenters.Users.GetUsers;

public class GetUsersPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetUsersOutputPort
{

    #region Methods

    Task IGetUsersOutputPort.PresentUsersAsync(IEnumerable<User> users, CancellationToken cancellationToken)
        => this.OkAsync(new GetUsersApiResponse()
        {
            Users = [.. users.Select(u => new UserSummaryDto()
            {
                Email = u.Email,
                FirstName = u.FirstName,
                FullName = u.UserName,
                LastName = u.LastName,
                UserID = u.UserID
            })]
        }, cancellationToken);

    #endregion Methods

}
