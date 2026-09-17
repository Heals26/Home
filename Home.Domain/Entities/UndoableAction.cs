namespace Home.Domain.Entities;

/// <summary>
/// What one request changed, kept while its undo is on offer so the token the device sent with it
/// can put everything back.
/// </summary>
public class UndoableAction
{

    #region Properties

    public long UndoableActionID { get; set; }

    /// <summary>
    /// False when the request did something no undo can put back, so an undo is refused rather than
    /// half done.
    /// </summary>
    public bool CanUndo { get; set; }

    /// <summary>
    /// Every row the request added, changed or held back from deleting, as JSON.
    /// </summary>
    public string Changes { get; set; } = string.Empty;

    public DateTime CreatedOnUTC { get; set; }
    public Household Household { get; set; } = null!;
    public Guid Token { get; set; }
    public DateTime? UndoneOnUTC { get; set; }

    #endregion Properties

}
