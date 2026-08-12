using Home.WebApi.UseCases.Lights.Models;

namespace Home.WebApi.UseCases.Lights.GetLights;

public class GetLightsApiResponse
{

    #region Properties

    /// <summary>
    /// Every light on the account, ordered by group then label.
    /// </summary>
    public List<LightDto> Lights { get; set; } = [];

    #endregion Properties

}
