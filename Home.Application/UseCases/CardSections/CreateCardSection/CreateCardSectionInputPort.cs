using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.CardSections.CreateCardSection;

public record CreateCardSectionInputPort(string Name) : IInputPort<ICreateCardSectionOutputPort>;