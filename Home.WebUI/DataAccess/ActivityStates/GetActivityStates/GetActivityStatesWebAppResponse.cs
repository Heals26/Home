namespace Home.WebUI.DataAccess.ActivityStates.GetActivityStates;

public class GetActivityStatesWebAppResponse
{

    #region Properties

    /// <summary>
    /// The available workflow states (kanban columns).
    /// </summary>
    public ICollection<ActivityStateDto> States { get; set; } = [];

    #endregion Properties

}

public class ActivityStateDto
{

    #region Properties

    /// <summary>
    /// The ID of the state.
    /// </summary>
    public long ActivityStateID { get; set; }

    /// <summary>
    /// The name of the state.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    #endregion Properties

}
