using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Devices.GetDevices;

public record GetDevicesInputPort() : IInputPort<IGetDevicesOutputPort>;
