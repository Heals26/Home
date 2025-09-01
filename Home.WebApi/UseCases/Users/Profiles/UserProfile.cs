using AutoMapper;
using Home.Application.UseCases.Users.CreateUser;
using Home.Application.UseCases.Users.UpdateUser;
using Home.WebApi.UseCases.Users.CreateUser;
using Home.WebApi.UseCases.Users.UpdateUser;

namespace Home.WebApi.UseCases.Users.Profiles;

public class UserProfile : Profile
{

    #region Constructors

    public UserProfile()
    {
        _ = this.CreateMap<CreateUserApiRequest, CreateUserInputPort>();
        _ = this.CreateMap<UpdateUserApiRequest, UpdateUserInputPort>()
            .ForMember(d => d.UserID, o => o.Ignore());

    }

    #endregion Constructors

}
