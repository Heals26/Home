namespace Home.Application.Services.Undo;

/// <summary>
/// Puts back what an undoable request changed, and clears away what no undo can reach any more.
/// </summary>
public interface IUndoStore
{

    #region Methods

    /// <summary>
    /// Deletes for good the rows held back by requests made before <paramref name="beforeUTC"/>,
    /// with the record of what those requests changed. Returns how many kinds of row could not be
    /// deleted, which are tried again next time.
    /// </summary>
    Task<int> PurgeAsync(DateTime beforeUTC, CancellationToken cancellationToken);

    /// <summary>
    /// Puts back what the request made with <paramref name="token"/> changed, as long as it was made
    /// no earlier than <paramref name="notBeforeUTC"/>. A value changed again since, by anyone, is
    /// left as it is now.
    /// </summary>
    Task<UndoOutcome> UndoAsync(long householdID, Guid token, DateTime notBeforeUTC, CancellationToken cancellationToken);

    #endregion Methods

}
