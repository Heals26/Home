using Home.WebUI.Infrastructure.ChangeTrackers;

namespace Home.WebUI.DataAccess.Users.UpdateUser;

public class UpdateUserWebAppRequest
{

    #region Properties

    public PropertyChangeTracker<string> Email { get; set; }
    public PropertyChangeTracker<string> FirstName { get; set; }
    public PropertyChangeTracker<string> LastName { get; set; }
    public PropertyChangeTracker<string> MiddleNames { get; set; }
    public PropertyChangeTracker<string> Password { get; set; }

    #endregion Properties

}
