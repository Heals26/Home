using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.MealPlanEntries.GetMealPlanEntries;

public record GetMealPlanEntriesInputPort(DateTime FromDate, DateTime ToDate) : IInputPort<IGetMealPlanEntriesOutputPort>;
