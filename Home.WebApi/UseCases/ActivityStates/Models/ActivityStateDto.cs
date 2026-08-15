namespace Home.WebApi.UseCases.ActivityStates.Models;

public class ActivityStateDto
{

    #region Properties

    public long ActivityStateID { get; set; }

    /// <summary>
    /// Landing in this column stamps the activity's completed date.
    /// </summary>
    public bool IsComplete { get; set; }

    public string Name { get; set; }

    /// <summary>
    /// Left-to-right order on the board.
    /// </summary>
    public int Sequence { get; set; }

    #endregion Properties

}
