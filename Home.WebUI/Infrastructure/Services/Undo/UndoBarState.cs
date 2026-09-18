namespace Home.WebUI.Infrastructure.Services.Undo;

/// <summary>
/// What the undo bar is showing.
/// </summary>
/// <param name="CanUndo">Undo is on offer, which it stops being once the API turns it down.</param>
/// <param name="IsUndoing">An undo is on its way to the API.</param>
/// <param name="Message">What was just done, or why it cannot be undone.</param>
public record UndoBarState(bool CanUndo, bool IsUndoing, string Message);
