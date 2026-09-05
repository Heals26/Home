using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Devices.SignOutDevice;

public record SignOutDeviceInputPort(long AuthenticationMetadataID) : IInputPort<ISignOutDeviceOutputPort>;
