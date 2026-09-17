namespace Home.Application.Services.Undo;

/// <summary>
/// The undo a request is made with. A device that means to offer Undo makes up a token and sends it
/// with the request, and everything the request changes is then kept so that token can put it back.
/// </summary>
public interface IUndoScope
{

    #region Properties

    /// <summary>
    /// The household the request acts for, which the undo is kept against.
    /// </summary>
    long? HouseholdID { get; set; }

    Guid? Token { get; set; }

    #endregion Properties

}
