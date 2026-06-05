using Home.WebApi.UseCases.ActivityStates.Models;

namespace Home.WebApi.UseCases.ActivityStates.GetActivityStates;

public class GetActivityStatesApiResponse
{

    #region Properties

    public ICollection<ActivityStateDto> States { get; set; }

    #endregion Properties

}
