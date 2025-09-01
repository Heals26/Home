using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.Users.UpdateUser;

public class UpdateUserInputPort : IInputPort<IUpdateUserOutputPort>
{

    #region Properties

    public PropertyChangeTracker<string> Email { get; set; }
    public PropertyChangeTracker<string> FirstName { get; set; }
    public PropertyChangeTracker<string> LastName { get; set; }
    public PropertyChangeTracker<string> MiddleNames { get; set; }
    public PropertyChangeTracker<string> Password { get; set; }
    public long UserID { get; set; }

    #endregion Properties

}
