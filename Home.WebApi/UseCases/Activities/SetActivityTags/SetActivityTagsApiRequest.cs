namespace Home.WebApi.UseCases.Activities.SetActivityTags;

/// <summary>
/// The complete set of tags the card should end up with — anything left out is removed from it.
/// </summary>
public record SetActivityTagsApiRequest(List<long> TagIDs);
