namespace Home.WebApi.UseCases.Recipes.GetRecipes;

public class GetRecipesApiResponse
{

    #region Properties

    public ICollection<GetRecipeDto> Recipes { get; set; }

    #endregion Properties

}

public class GetRecipeDto
{

    #region Properties

    public string Name { get; set; }
    public long RecipeID { get; set; }
    public string Url { get; set; }

    #endregion Properties

}
