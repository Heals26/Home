using Home.Domain.Entities;

namespace Home.Domain.Services.Users
{

    public interface IPasswordService
    {

        #region Methods

        void SetPassword(User user, string password);
        Task<bool> VerifyPassword(User user, string password, CancellationToken cancellationToken);

        #endregion Methods

    }

}
