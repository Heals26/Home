using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Devices.SignOutOtherDevices;

public record SignOutOtherDevicesInputPort() : IInputPort<ISignOutOtherDevicesOutputPort>;
