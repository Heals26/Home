namespace Home.Domain.Enumerations;

/// <summary>
/// What someone says their feedback is. Numbered from one, so a request that never chose a kind
/// fails validation instead of arriving as a problem.
/// </summary>
public enum FeedbackKind
{
    Problem = 1,
    Idea = 2,
}
