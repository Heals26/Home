using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.WebApi.UseCases.Users.UpdateUser;

public class UpdateUserApiRequest
{

    #region Properties

    public PropertyChangeTracker<string> Email { get; set; }
    public PropertyChangeTracker<string> FirstName { get; set; }
    public PropertyChangeTracker<string> LastName { get; set; }
    public PropertyChangeTracker<string> MiddleNames { get; set; }
    public PropertyChangeTracker<string> Password { get; set; }

    #endregion Properties

}
