using AutoMapper;
using Home.Domain.Entities;
using Home.WebApi.UseCases.Recipes.GetRecipes;

namespace Home.WebApi.Infrastructure.AutoMapper.ControllerProfiles;

public class RecipesProfile : Profile
{

    #region Constructors

    public RecipesProfile()
    {
        _ = this.CreateMap<IEnumerable<Recipe>, GetRecipesApiResponse>()
            .ForMember(d => d.Recipes, o => o.MapFrom(s => s));

        _ = this.CreateMap<Recipe, GetRecipeDto>();
    }

    #endregion Constructors

}
