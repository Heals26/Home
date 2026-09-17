namespace Home.Application.Services.Undo;

public enum UndoOutcome
{

    /// <summary>
    /// The request did something that cannot be put back, so nothing is put back rather than half
    /// of it.
    /// </summary>
    CannotUndo,

    Expired,
    NotFound,
    Undone

}
