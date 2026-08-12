using AutoMapper;
using Home.Application.UseCases.LightSchedules.RunDueLightSchedules;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.LightSchedules.RunDueLightSchedules;

/// <summary>
/// Not reached over HTTP — the background runner reads the counts off it directly, so this
/// presents into properties rather than an <c>IActionResult</c>.
/// </summary>
public class RunDueLightSchedulesPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IRunDueLightSchedulesOutputPort
{

    #region Properties

    public int Failed { get; private set; }

    public int Fired { get; private set; }

    #endregion Properties

    #region Methods

    Task IRunDueLightSchedulesOutputPort.PresentSchedulesRunAsync(int fired, int failed, CancellationToken cancellationToken)
    {
        this.Fired = fired;
        this.Failed = failed;

        return Task.CompletedTask;
    }

    #endregion Methods

}
