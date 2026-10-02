using FluentAssertions;
using Home.Application.UseCases.Support.CreateFeedback;
using Home.Domain.Enumerations;

namespace Home.Application.Tests.UseCases.Support.CreateFeedback;

/// <summary>
/// The pipeline runs a validator before the interactor, so an interactor test never reaches one.
/// These drive it directly.
/// </summary>
public class CreateFeedbackInputPortValidatorTests
{

    #region Methods

    private static bool IsValid(CreateFeedbackInputPort inputPort)
        => new CreateFeedbackInputPortValidator().Validate(inputPort).IsValid;

    [Theory]
    [InlineData(FeedbackKind.Idea)]
    [InlineData(FeedbackKind.Problem)]
    public void Validate_AcceptsAProblemOrAnIdea(FeedbackKind kind)
        => IsValid(new("The timer is too quiet", kind, "/recipes/12/cook", 768, 1024)).Should().BeTrue();

    [Fact]
    public void Validate_AcceptsTwoThousandCharactersAndNoMore()
    {
        _ = IsValid(new(new string('a', 2000), FeedbackKind.Idea, null, null, null)).Should().BeTrue();
        _ = IsValid(new(new string('a', 2001), FeedbackKind.Idea, null, null, null)).Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public void Validate_RefusesAKindThatIsNeitherAProblemNorAnIdea(int kind)
        => IsValid(new("The timer is too quiet", (FeedbackKind)kind, null, null, null)).Should().BeFalse();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_RefusesFeedbackWithNothingWritten(string content)
        => IsValid(new(content, FeedbackKind.Idea, null, null, null)).Should().BeFalse();

    #endregion Methods

}
