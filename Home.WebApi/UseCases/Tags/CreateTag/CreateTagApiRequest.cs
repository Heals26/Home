namespace Home.WebApi.UseCases.Tags.CreateTag;

/// <summary>
/// Colour must be a hex value in the form #RRGGBB.
/// </summary>
public record CreateTagApiRequest(
    string Colour,
    string Name);
