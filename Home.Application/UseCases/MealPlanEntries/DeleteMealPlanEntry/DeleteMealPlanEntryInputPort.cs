using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.MealPlanEntries.DeleteMealPlanEntry;

public record DeleteMealPlanEntryInputPort(long MealPlanEntryID) : IInputPort<IDeleteMealPlanEntryOutputPort>;
