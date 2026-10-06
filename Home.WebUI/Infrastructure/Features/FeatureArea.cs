namespace Home.WebUI.Infrastructure.Features;

/// <summary>
/// One part of the app and everything it can do.
/// </summary>
/// <param name="Eyebrow">The part's name, as the navigation calls it.</param>
/// <param name="Headline">What the part is for, in one line.</param>
/// <param name="Href">The screen itself.</param>
/// <param name="HueClass">The part's own colour, which is how the app says where you are.</param>
/// <param name="Features">What it can do.</param>
public record FeatureArea(
    string Eyebrow,
    string Headline,
    string Href,
    string HueClass,
    IReadOnlyList<Feature> Features);
