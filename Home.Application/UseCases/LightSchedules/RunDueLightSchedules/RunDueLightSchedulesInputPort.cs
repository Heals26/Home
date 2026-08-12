using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.LightSchedules.RunDueLightSchedules;

/// <summary>
/// Fired by the background runner, not by a user. Takes no time argument — the interactor reads
/// the clock through <see cref="TimeProvider"/>, which tests substitute.
/// </summary>
public record RunDueLightSchedulesInputPort() : IInputPort<IRunDueLightSchedulesOutputPort>;
