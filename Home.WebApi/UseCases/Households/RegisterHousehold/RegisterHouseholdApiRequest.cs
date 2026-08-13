namespace Home.WebApi.UseCases.Households.RegisterHousehold;

public record RegisterHouseholdApiRequest(
    string Email,
    string FirstName,
    string HouseholdName,
    string LastName,
    string Password);
