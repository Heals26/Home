using Home.WebApi.UseCases.Lights.Models;

namespace Home.WebApi.UseCases.Lights.GetLights;

public class GetLightsApiResponse
{

    #region Properties

    /// <summary>
    /// Every group in the household with its lights, in display order. Served from Home's own
    /// records — call the sync endpoint to refresh them from the provider.
    /// </summary>
    public List<LightGroupDto> Groups { get; set; } = [];

    #endregion Properties

}
