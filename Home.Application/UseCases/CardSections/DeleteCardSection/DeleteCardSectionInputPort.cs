using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.CardSections.DeleteCardSection;

public record DeleteCardSectionInputPort(long CardSectionID) : IInputPort<IDeleteCardSectionOutputPort>;