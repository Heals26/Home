using AutoMapper;
using Home.Domain.Entities;
using Home.WebApi.UseCases.ShoppingLists.GetShoppingList;
using Home.WebApi.UseCases.ShoppingLists.GetShoppingLists;
using Home.WebApi.UseCases.ShoppingLists.Models;

namespace Home.WebApi.Infrastructure.AutoMapper.ControllerProfiles;

public class ShoppingListsProfile : Profile
{

    #region Constructors

    public ShoppingListsProfile()
    {
        // Without this the list's Items have no element map, and fetching a shopping list
        // fails at the point of use rather than at startup.
        _ = this.CreateMap<ShoppingListItem, ShoppingListItemDto>();

        _ = this.CreateMap<ShoppingList, GetShoppingListApiResponse>();
        _ = this.CreateMap<IEnumerable<ShoppingList>, GetShoppingListsApiResponse>()
            .ForMember(d => d.ShoppingLists, o => o.MapFrom(s => s));

        _ = this.CreateMap<ShoppingList, GetShoppingListDto>()
            .ForMember(d => d.ItemCount, o => o.MapFrom(s => s.Items.Count));
    }

    #endregion Constructors

}
