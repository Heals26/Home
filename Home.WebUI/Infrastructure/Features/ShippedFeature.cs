namespace Home.WebUI.Infrastructure.Features;

/// <summary>
/// Something built lately, with the day it landed. This is the half of the page that answers "I
/// forgot we did that".
/// </summary>
/// <param name="On">The day it shipped.</param>
/// <param name="Name">What it is called.</param>
/// <param name="What">What changed, in one plain sentence.</param>
public record ShippedFeature(DateOnly On, string Name, string What);
