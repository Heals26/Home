using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Recipes;

public partial class RecipeSummaryStrip
{

    #region Properties

    [Parameter] public string? Class { get; set; }
    [Parameter] public long? Complexity { get; set; }
    [Parameter] public int? CookMinutes { get; set; }
    [Parameter] public int? PrepMinutes { get; set; }
    [Parameter] public int? Servings { get; set; }

    #endregion Properties

    #region Methods

    private bool HasAnything()
        => this.PrepMinutes is > 0
            || this.CookMinutes is > 0
            || this.Servings is > 0
            || !string.IsNullOrEmpty(this.ComplexityName());

    private string ComplexityName()
        => RecipeDisplayLogic.DescribeComplexity(this.Complexity);

    #endregion Methods

}
