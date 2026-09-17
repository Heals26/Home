namespace Home.Domain.Deletions;

/// <summary>
/// A row that a delete can take back. While an undo is still on offer, the delete only stamps
/// <see cref="DeletedOnUTC"/>: every query leaves the row out as if it were gone, and the purge
/// deletes it for good once the undo has run out.
/// </summary>
public interface ISoftDeletable
{

    #region Properties

    DateTime? DeletedOnUTC { get; set; }

    #endregion Properties

}
