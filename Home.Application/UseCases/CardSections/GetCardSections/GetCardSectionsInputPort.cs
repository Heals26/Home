using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.CardSections.GetCardSections;

public record GetCardSectionsInputPort() : IInputPort<IGetCardSectionsOutputPort>;