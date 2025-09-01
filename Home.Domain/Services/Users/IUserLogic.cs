using Home.Domain.Entities;

namespace Home.Domain.Services.Users;


public interface IUserLogic
{

    #region Methods

    User CreateUser();
    User UpdateUser();
    User DeleteUser(long userID);
    User GetUser(long userID);
    User GetUsers();

    #endregion Methods

}
