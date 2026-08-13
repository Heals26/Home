using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Households.RegisterHousehold;

public record RegisterHouseholdInputPort(
    string Email,
    string FirstName,
    string HouseholdName,
    string LastName,
    string Password) : IInputPort<IRegisterHouseholdOutputPort>;
