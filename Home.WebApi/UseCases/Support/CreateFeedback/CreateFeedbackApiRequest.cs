using Home.Domain.Enumerations;

namespace Home.WebApi.UseCases.Support.CreateFeedback;

public record CreateFeedbackApiRequest(string Content, FeedbackKind Kind, string? Page, int? ScreenHeight, int? ScreenWidth);
