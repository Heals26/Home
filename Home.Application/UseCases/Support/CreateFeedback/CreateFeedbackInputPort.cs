using CleanArchitecture.Mediator;
using Home.Domain.Enumerations;

namespace Home.Application.UseCases.Support.CreateFeedback;

public record CreateFeedbackInputPort(
    string Content,
    FeedbackKind Kind,
    string? Page,
    int? ScreenHeight,
    int? ScreenWidth)
    : IInputPort<ICreateFeedbackOutputPort>;
