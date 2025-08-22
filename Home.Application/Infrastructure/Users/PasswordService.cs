using Home.Application.Services.Database;
using Home.Domain.Entities;
using Home.Domain.Services.Users;
using Microsoft.AspNetCore.Identity;

namespace Home.Application.Infrastructure.Users
{

    public class PasswordService(IPersistenceContext persistenceContext, PasswordHasher<User> passwordHasher) : IPasswordService
    {

        #region Methods

        void IPasswordService.SetPassword(User user, string password)
        {
            user.Password = passwordHasher.HashPassword(user, password);
            user.PasswordLastChanged = DateTime.UtcNow;
        }

        async Task<bool> IPasswordService.VerifyPassword(User user, string password, CancellationToken cancellationToken)
        {
            var _Result = passwordHasher.VerifyHashedPassword(user, user.Password, password);

            if (_Result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.Password = passwordHasher.HashPassword(user, password);

                _ = await persistenceContext.SaveChangesAsync(cancellationToken);
            }

            return _Result == PasswordVerificationResult.Success || _Result == PasswordVerificationResult.SuccessRehashNeeded;
        }

        #endregion Methods

    }

}
