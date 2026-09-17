namespace Home.Persistence.Undo;

public enum UndoChangeKind
{

    Added,

    /// <summary>
    /// Held back from deleting: only its <c>DeletedOnUTC</c> was set.
    /// </summary>
    Deleted,

    Modified

}
