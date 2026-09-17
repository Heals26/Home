namespace Home.Persistence.Undo;

/// <summary>
/// One row an undoable request touched: which row it was, and for a row it changed, what each value
/// was before and after.
/// </summary>
public record UndoChange(UndoChangeKind Kind, string EntityType, IReadOnlyList<UndoValue> Key, IReadOnlyList<UndoValueChange> Values);
