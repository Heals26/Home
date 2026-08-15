using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Activities.SetActivityTags;

/// <summary>
/// TagIDs is the complete set the card should end up with — anything missing from it is removed.
/// Replacing the whole set keeps two tablets editing the same card from ending up with different
/// answers, which an add-then-remove pair cannot promise.
/// </summary>
public record SetActivityTagsInputPort(
    long ActivityID,
    ICollection<long> TagIDs)
    : IInputPort<ISetActivityTagsOutputPort>;
