using AutoMapper;
using Home.Application.UseCases.Users.CreateUser;
using Home.WebApi.UseCases.Users.CreateUser;

namespace Home.WebApi.UseCases.Users.Profiles;

public class UserProfile : Profile
{

    #region Constructors

    public UserProfile()
    {
        this.CreateMap<CreateUserApiRequest, CreateUserInputPort>();

    }

    #endregion Constructors

}
