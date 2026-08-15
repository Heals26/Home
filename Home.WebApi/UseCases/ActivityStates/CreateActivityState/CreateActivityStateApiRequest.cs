namespace Home.WebApi.UseCases.ActivityStates.CreateActivityState;

/// <summary>
/// A new column is added at the right-hand end of the board; use the update to reorder it.
/// </summary>
public record CreateActivityStateApiRequest(
    string Name,
    bool IsComplete);
