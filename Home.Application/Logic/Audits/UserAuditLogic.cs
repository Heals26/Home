using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;

namespace Home.Application.Logic.Audits;

/// <summary>
/// Member history. The first member of a household is written while nobody is signed in, so the
/// doer and the household are read off the member rather than the session.
/// </summary>
public class UserAuditLogic(
    IAuthorisationService authorisationService,
    IPersistenceContext persistenceContext,
    TimeProvider timeProvider)
    : AuditBase<User>(authorisationService, persistenceContext, timeProvider)
{

    #region Fields

    private User? m_Subject;

    #endregion Fields

    #region Properties

    protected override ResourceTypeSE ResourceType => ResourceTypeSE.User;

    #endregion Properties

    #region Methods

    protected override string Describe(User user, EntityState entityState)
    {
        this.m_Subject = user;

        if (entityState == EntityState.Added)
            return $"added {user.FirstName} to the household";

        if (entityState == EntityState.Deleted)
            return $"removed {user.FirstName} from the household";

        if (this.HasChanged(user, nameof(User.Password)))
            return this.HasChanged(user, nameof(User.Email)) && this.OriginalValue(user, nameof(User.Email)) == null
                ? $"gave {user.FirstName} a sign-in"
                : $"changed {user.FirstName}'s password";

        if (this.HasChanged(user, nameof(User.Email)) && user.Email == null)
            return $"took away {user.FirstName}'s sign-in";

        return $"changed {user.FirstName}'s details";
    }

    /// <summary>
    /// During first-run registration there is no session; the household is the new member's own.
    /// </summary>
    protected override Household? GetHousehold()
        => TryGet(base.GetHousehold) ?? this.m_Subject?.Household;

    protected override User? GetUser()
        => TryGet(base.GetUser) ?? this.m_Subject;

    private static T? TryGet<T>(Func<T?> read) where T : class
    {
        try
        {
            return read();
        }
        catch (Exception _Exception) when (_Exception is UnauthorizedAccessException or InvalidOperationException or ArgumentNullException or NullReferenceException)
        {
            // No signed-in principal: registration, or a background run.
            return null;
        }
    }

    #endregion Methods

}
